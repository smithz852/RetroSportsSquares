using Microsoft.Extensions.Logging;
using RSS.DTOs;

namespace RSS_Services
{
    // Single pipeline from a score update to persisted period winners and their
    // notification emails. Used by both the refetch automation and the dev trigger
    // endpoint so winners and emails always flow through the same code path.
    public class GameResultProcessor
    {
        private readonly SquareServices _squareServices;
        private readonly GameNotificationService _notifications;
        private readonly ILogger<GameResultProcessor> _logger;

        public GameResultProcessor(SquareServices squareServices, GameNotificationService notifications, ILogger<GameResultProcessor> logger)
        {
            _squareServices = squareServices;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task ProcessQuarterlyWinnersAsync(SportScoreUpdateDTO scoreData, Guid sportsGameId)
        {
            var squareGames = await _squareServices.GetSquareGamesBySportsGameId(sportsGameId);
            foreach (var squareGame in squareGames)
            {
                var winner = await _squareServices.DetermineQuarterlyWinner(scoreData, squareGame.Id);
                if (winner?.UserId == null) continue;

                var result = await _squareServices.SaveQuarterlyWinner(winner, squareGame.Id);
                if (result == null) continue; // period already resolved on a prior tick — no emails

                await _notifications.SendPeriodWinEmailAsync(squareGame.Id, result.Period, result.WinnerApplicationUserId);

                if (result.GameCompleted)
                    await _notifications.SendGameRecapEmailsAsync(squareGame.Id);
            }
        }
    }
}
