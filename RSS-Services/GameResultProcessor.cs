using Microsoft.Extensions.Logging;
using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using System.Linq;

namespace RSS_Services
{
    // Single pipeline from a score update to persisted period winners and their
    // notification emails. Used by both the refetch automation and the dev trigger
    // endpoint so winners and emails always flow through the same code path.
    public class GameResultProcessor
    {
        private readonly SquareServices _squareServices;
        private readonly GameNotificationService _notifications;
        private readonly GameSettlementService _settlement;
        private readonly ILogger<GameResultProcessor> _logger;

        public GameResultProcessor(SquareServices squareServices, GameNotificationService notifications, GameSettlementService settlement, ILogger<GameResultProcessor> logger)
        {
            _squareServices = squareServices;
            _notifications = notifications;
            _settlement = settlement;
            _logger = logger;
        }

        public async Task ProcessQuarterlyWinnersAsync(SportScoreUpdateDTO scoreData, Guid sportsGameId)
        {
            var squareGames = await _squareServices.GetSquareGamesBySportsGameId(sportsGameId);
            var currentPeriod = SquareServices.GetCurrentGamePeriodIndex(scoreData.Status, scoreData.SportType);

            foreach (var squareGame in squareGames)
            {
                // Save every resolved period, including unclaimed (null) ones, so
                // null-reactive modes (Push pot, arrows, bombs) see them live.
                var winner = await _squareServices.DetermineQuarterlyWinner(scoreData, squareGame.Id);
                if (winner != null)
                {
                    var result = await _squareServices.SaveQuarterlyWinner(winner, squareGame.Id);
                    if (result != null) // null = unclaimed period or already resolved — no email
                    {
                        await _notifications.SendPeriodWinEmailAsync(squareGame.Id, result.Period, result.WinnerApplicationUserId);

                        // Thief: if this win completed an arrow, the victim's squares
                        // move to the shooter NOW so later periods resolve against the
                        // new ownership. ThiefWalk is the same rules the settlement
                        // engine replays, so the two can never disagree.
                        if (squareGame.PayoutMode == PayoutModes.Thief)
                        {
                            var elimination = ThiefWalk
                                .Analyze(squareGame.PeriodWinners, squareGame.PeriodCount)
                                .FirstOrDefault(e => e.Type == ThiefEventType.Elimination && e.Period == result.Period);
                            if (elimination != null)
                                await _squareServices.EliminateThiefVictimAsync(squareGame.Id, elimination.TargetId!, elimination.ActorId);
                        }
                    }
                }

                // Terminal status: the sports game is past this board's final period.
                // Complete the game even when the final period's square was unclaimed,
                // then send recaps (RecapEmailSent guard keeps this at-most-once).
                // Must run AFTER the winner save above: completion backfills missing
                // periods as null, which would otherwise mark the final period as
                // already resolved and swallow a legitimately claimed winner.
                if (currentPeriod > squareGame.PeriodCount)
                {
                    await _squareServices.CompleteGameAsync(squareGame.Id);
                    // Settle before the recap so recap emails describe final,
                    // credited amounts (SettlementCompleted keeps this at-most-once).
                    await _settlement.SettleGameAsync(squareGame.Id);
                    await _notifications.SendGameRecapEmailsAsync(squareGame.Id);
                }
            }
        }
    }
}
