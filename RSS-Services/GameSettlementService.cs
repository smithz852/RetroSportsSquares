using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RSS_Services
{
    // Escrow settlement: wallets only move here, once, when a game completes.
    // Guarded by SquareGames.SettlementCompleted; the flag, ledger rows, and
    // balance credits commit in a single transaction, so a crash retries cleanly
    // on the next tick. Coins are credited only when the wallet actually escrowed
    // wagers (public game with Wager ledger rows) — private games and games
    // created before the wallet existed settle on paper only.
    public class GameSettlementService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<GameSettlementService> _logger;

        public GameSettlementService(AppDbContext appDbContext, ILogger<GameSettlementService> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        // Best-effort like the notification services: failures are logged and
        // swallowed so settlement can never break the score-refetch loop. An
        // unset flag means the next tick simply retries.
        public async Task SettleGameAsync(Guid squareGameId)
        {
            try
            {
                var game = await _appDbContext.SquareGames
                    .Include(g => g.GamePlayers)
                    .Include(g => g.GameSquares)
                    .FirstOrDefaultAsync(g => g.Id == squareGameId);
                if (game == null || !game.IsCompleted || game.SettlementCompleted) return;

                var squareCountsByUser = GetSquareCountsByUser(game);
                var lines = SettlementEngine.ComputeSettlement(
                    game.PayoutMode, game.PeriodWinners, game.PeriodCount, game.PricePerSquare, squareCountsByUser);

                var hasEscrow = game.IsPublic && await _appDbContext.CoinTransactions
                    .AnyAsync(t => t.SquareGameId == game.Id && t.Type == CoinTransactionTypes.Wager);

                await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    game.SettlementCompleted = true;

                    // Thief: make sure every arrow victim carries the flag — mid-game
                    // eliminations were flagged live, but a game-end arrow (trailing
                    // null periods) only resolves here.
                    if (game.PayoutMode == PayoutModes.Thief)
                    {
                        var victims = ThiefWalk.Analyze(game.PeriodWinners, game.PeriodCount)
                            .Where(e => e.Type == ThiefEventType.Elimination)
                            .Select(e => e.TargetId!)
                            .ToHashSet();
                        foreach (var player in game.GamePlayers.Where(p => victims.Contains(p.ApplicationUserId)))
                            player.IsEliminated = true;
                    }

                    if (hasEscrow)
                    {
                        foreach (var line in lines)
                        {
                            _appDbContext.CoinTransactions.Add(new CoinTransaction
                            {
                                ApplicationUserId = line.UserId,
                                SquareGameId = game.Id,
                                Amount = line.Amount,
                                Type = line.Type,
                                Period = line.Period
                            });

                            await _appDbContext.Users
                                .Where(u => u.Id == line.UserId)
                                .ExecuteUpdateAsync(s => s.SetProperty(u => u.CoinBalance, u => u.CoinBalance + line.Amount));
                        }
                    }

                    await _appDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Settled game {GameId} ({Mode}): {LineCount} lines totaling {Total}, coins credited: {Credited}",
                        game.Id, game.PayoutMode, lines.Count, lines.Sum(l => l.Amount), hasEscrow);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to settle game {GameId}; will retry on the next tick", squareGameId);
            }
        }

        public static System.Collections.Generic.Dictionary<string, int> GetSquareCountsByUser(SquareGames game)
        {
            var userIdByPlayerId = game.GamePlayers.ToDictionary(gp => gp.Id, gp => gp.ApplicationUserId);
            return game.GameSquares
                .Where(gs => gs.GamePlayerId != null && userIdByPlayerId.ContainsKey(gs.GamePlayerId.Value))
                .GroupBy(gs => userIdByPlayerId[gs.GamePlayerId.Value])
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
