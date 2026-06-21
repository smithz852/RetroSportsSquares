using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_Services.DTOs;

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
                    gp.Game.WinnerQ1Id,
                    gp.Game.WinnerQ2Id,
                    gp.Game.WinnerQ3Id,
                    gp.Game.WinnerQ4Id,
                    gp.Game.Q1Skipped,
                    gp.Game.Q2Skipped,
                    gp.Game.Q3Skipped,
                    PlayerSquaresCount = gp.GamePlayerSquares.Count(),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null)
                })
                .ToListAsync();

            var periodsWon = gameData.Sum(g =>
                (g.WinnerQ1Id == userId && !g.Q1Skipped ? 1 : 0) +
                (g.WinnerQ2Id == userId && !g.Q2Skipped ? 1 : 0) +
                (g.WinnerQ3Id == userId && !g.Q3Skipped ? 1 : 0) +
                (g.WinnerQ4Id == userId ? 1 : 0));

            var totalSquaresClaimed = gameData.Sum(g => g.PlayerSquaresCount);
            var totalWagered = gameData.Sum(g => g.PricePerSquare * g.PlayerSquaresCount);

            var wagersWon = gameData.Sum(g =>
            {
                var won = (g.WinnerQ1Id == userId && !g.Q1Skipped ? 1 : 0) +
                          (g.WinnerQ2Id == userId && !g.Q2Skipped ? 1 : 0) +
                          (g.WinnerQ3Id == userId && !g.Q3Skipped ? 1 : 0) +
                          (g.WinnerQ4Id == userId ? 1 : 0);
                if (won == 0) return 0;

                var totalPeriods =
                    (g.WinnerQ1Id != null && !g.Q1Skipped ? 1 : 0) +
                    (g.WinnerQ2Id != null && !g.Q2Skipped ? 1 : 0) +
                    (g.WinnerQ3Id != null && !g.Q3Skipped ? 1 : 0) +
                    (g.WinnerQ4Id != null ? 1 : 0);
                if (totalPeriods == 0) return 0;

                var totalPool = g.PricePerSquare * g.TotalGameSquaresCount;
                return (totalPool / totalPeriods) * won;
            });

            var totalPeriodsPlayed = gameData.Sum(g =>
                (g.WinnerQ1Id != null && !g.Q1Skipped ? 1 : 0) +
                (g.WinnerQ2Id != null && !g.Q2Skipped ? 1 : 0) +
                (g.WinnerQ3Id != null && !g.Q3Skipped ? 1 : 0) +
                (g.WinnerQ4Id != null ? 1 : 0));

            return new PlayerStatsDTO
            {
                PeriodsWon = periodsWon,
                TotalWagered = totalWagered,
                WagersWon = wagersWon,
                WinRate = totalPeriodsPlayed > 0 ? Math.Round((double)periodsWon / totalPeriodsPlayed, 3) : 0,
                TotalSquaresClaimed = totalSquaresClaimed
            };
        }

        // A game is "current" when Q4 has no winner yet (final period unresolved)
        public async Task<List<CurrentGameSummaryDTO>> GetCurrentGamesAsync(string userId)
        {
            return await _context.GamePlayers
                .Where(gp => gp.ApplicationUserId == userId && gp.Game.WinnerQ4Id == null)
                .Select(gp => new CurrentGameSummaryDTO
                {
                    GameId = gp.GameId,
                    GameName = gp.Game.GameName,
                    GameType = gp.Game.GameType,
                    PricePerSquare = gp.Game.PricePerSquare,
                    SquaresClaimed = gp.GamePlayerSquares.Count(),
                    IsHost = gp.IsHost,
                    IsOpen = gp.Game.isOpen,
                    SelectionPhaseActive = gp.Game.SelectionPhaseActive
                })
                .ToListAsync();
        }

        // A game is "past" when Q4 winner has been recorded
        public async Task<PaginatedPastGamesDTO> GetPastGamesAsync(string userId, int page, int pageSize)
        {
            var query = _context.GamePlayers
                .Where(gp => gp.ApplicationUserId == userId && gp.Game.WinnerQ4Id != null);

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
                    gp.Game.WinnerQ1Id,
                    gp.Game.WinnerQ2Id,
                    gp.Game.WinnerQ3Id,
                    gp.Game.WinnerQ4Id,
                    gp.Game.Q1Skipped,
                    gp.Game.Q2Skipped,
                    gp.Game.Q3Skipped,
                    PlayerSquaresCount = gp.GamePlayerSquares.Count(),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null)
                })
                .ToListAsync();

            var summaries = rawGames.Select(g =>
            {
                var periodsWon =
                    (g.WinnerQ1Id == userId && !g.Q1Skipped ? 1 : 0) +
                    (g.WinnerQ2Id == userId && !g.Q2Skipped ? 1 : 0) +
                    (g.WinnerQ3Id == userId && !g.Q3Skipped ? 1 : 0) +
                    (g.WinnerQ4Id == userId ? 1 : 0);

                var totalPeriods =
                    (g.WinnerQ1Id != null && !g.Q1Skipped ? 1 : 0) +
                    (g.WinnerQ2Id != null && !g.Q2Skipped ? 1 : 0) +
                    (g.WinnerQ3Id != null && !g.Q3Skipped ? 1 : 0) +
                    (g.WinnerQ4Id != null ? 1 : 0);

                var totalPool = g.PricePerSquare * g.TotalGameSquaresCount;
                var totalWon = totalPeriods > 0 ? (totalPool / totalPeriods) * periodsWon : 0;

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
