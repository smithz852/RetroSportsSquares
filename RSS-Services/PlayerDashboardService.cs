using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS_Services
{
    public class PlayerDashboardService
    {
        private readonly AppDbContext _context;

        public PlayerDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerStatsDTO> GetStatsAsync(string userId)
        {
            var gameData = await _context.GamePlayers
                .Where(gp => gp.ApplicationUserId == userId)
                .Select(gp => new
                {
                    gp.Game.PricePerSquare,
                    gp.Game.PeriodWinners,
                    gp.Game.PeriodCount,
                    gp.Game.PayoutMode,
                    gp.Game.IsCompleted,
                    // Wager stats count the original buyer — Thief eliminations
                    // reassign GamePlayerId but never rewrite who paid.
                    PlayerSquaresCount = gp.Game.GameSquares.Count(gs => (gs.OriginalGamePlayerId ?? gs.GamePlayerId) == gp.Id),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null),
                    ClaimedSquareOwners = gp.Game.GameSquares
                        .Where(gs => gs.GamePlayerId != null)
                        .Select(gs => gs.GamePlayer.ApplicationUserId)
                        .ToList()
                })
                .ToListAsync();

            var periodsWon = gameData.Sum(g =>
                g.PeriodWinners.Values.Count(v => v == userId));

            var totalSquaresClaimed = gameData.Sum(g => g.PlayerSquaresCount);
            var totalWagered = gameData.Sum(g => g.PricePerSquare * g.PlayerSquaresCount);

            // Completed games settle through the engine (mode-correct: Fair boosts,
            // Push carries). In-progress games use the flat per-period share as a
            // conservative floor — their real amounts aren't final until settlement.
            var wagersWon = gameData.Sum(g =>
            {
                if (g.IsCompleted)
                    return SettlementEngine.GetUserPrizeTotal(g.PayoutMode, g.PeriodWinners, g.PeriodCount, g.PricePerSquare, g.ClaimedSquareOwners, userId);

                var won = g.PeriodWinners.Values.Count(v => v == userId);
                return won * PayoutCalculator.GetPayoutPerPeriod(g.PricePerSquare, g.TotalGameSquaresCount, g.PeriodCount);
            });

            var totalPeriodsPlayed = gameData.Sum(g =>
                g.PeriodWinners.Values.Count(v => v != null));

            return new PlayerStatsDTO
            {
                PeriodsWon = periodsWon,
                TotalWagered = totalWagered,
                WagersWon = wagersWon,
                WinRate = totalPeriodsPlayed > 0 ? Math.Round((double)periodsWon / totalPeriodsPlayed, 3) : 0,
                TotalSquaresClaimed = totalSquaresClaimed
            };
        }

        public async Task<List<CurrentGameSummaryDTO>> GetCurrentGamesAsync(string userId)
        {
            return await _context.GamePlayers
                .Where(gp => gp.ApplicationUserId == userId && !gp.Game.IsCompleted)
                .Select(gp => new CurrentGameSummaryDTO
                {
                    GameId = gp.GameId,
                    GameName = gp.Game.GameName,
                    GameType = gp.Game.GameType,
                    PricePerSquare = gp.Game.PricePerSquare,
                    SquaresClaimed = gp.Game.GameSquares.Count(gs => (gs.OriginalGamePlayerId ?? gs.GamePlayerId) == gp.Id),
                    IsHost = gp.IsHost,
                    IsOpen = gp.Game.isOpen,
                    SelectionPhaseActive = gp.Game.SelectionPhaseActive
                })
                .ToListAsync();
        }

        public async Task<PaginatedPastGamesDTO> GetPastGamesAsync(string userId, int page, int pageSize)
        {
            var query = _context.GamePlayers
                .Where(gp => gp.ApplicationUserId == userId && gp.Game.IsCompleted);

            var totalCount = await query.CountAsync();

            var rawGames = await query
                .OrderByDescending(gp => gp.Game.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(gp => new
                {
                    gp.GameId,
                    gp.Game.GameName,
                    gp.Game.GameType,
                    gp.Game.PricePerSquare,
                    gp.Game.CreatedAt,
                    gp.Game.PeriodWinners,
                    gp.Game.PeriodCount,
                    gp.Game.PayoutMode,
                    PlayerSquaresCount = gp.Game.GameSquares.Count(gs => (gs.OriginalGamePlayerId ?? gs.GamePlayerId) == gp.Id),
                    ClaimedSquareOwners = gp.Game.GameSquares
                        .Where(gs => gs.GamePlayerId != null)
                        .Select(gs => gs.GamePlayer.ApplicationUserId)
                        .ToList()
                })
                .ToListAsync();

            var summaries = rawGames.Select(g =>
            {
                var periodsWon = g.PeriodWinners.Values.Count(v => v == userId);
                // Past games are completed, so the engine gives exact, mode-correct amounts.
                var totalWon = SettlementEngine.GetUserPrizeTotal(g.PayoutMode, g.PeriodWinners, g.PeriodCount, g.PricePerSquare, g.ClaimedSquareOwners, userId);

                return new PastGameSummaryDTO
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    GameType = g.GameType,
                    PricePerSquare = g.PricePerSquare,
                    SquaresClaimed = g.PlayerSquaresCount,
                    PeriodsWon = periodsWon,
                    TotalWagered = g.PricePerSquare * g.PlayerSquaresCount,
                    TotalWon = totalWon,
                    CreatedAt = g.CreatedAt
                };
            }).ToList();

            return new PaginatedPastGamesDTO
            {
                Games = summaries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
