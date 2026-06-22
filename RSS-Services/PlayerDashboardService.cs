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
                    gp.Game.PeriodWinners,
                    PlayerSquaresCount = gp.GamePlayerSquares.Count(),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null)
                })
                .ToListAsync();

            var periodsWon = gameData.Sum(g =>
                g.PeriodWinners.Values.Count(v => v == userId));

            var totalSquaresClaimed = gameData.Sum(g => g.PlayerSquaresCount);
            var totalWagered = gameData.Sum(g => g.PricePerSquare * g.PlayerSquaresCount);

            var wagersWon = gameData.Sum(g =>
            {
                var won = g.PeriodWinners.Values.Count(v => v == userId);
                if (won == 0) return 0;

                var totalPeriods = g.PeriodWinners.Values.Count(v => v != null);
                if (totalPeriods == 0) return 0;

                var totalPool = g.PricePerSquare * g.TotalGameSquaresCount;
                return (totalPool / totalPeriods) * won;
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
                    SquaresClaimed = gp.GamePlayerSquares.Count(),
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
                    PlayerSquaresCount = gp.GamePlayerSquares.Count(),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null)
                })
                .ToListAsync();

            var summaries = rawGames.Select(g =>
            {
                var periodsWon = g.PeriodWinners.Values.Count(v => v == userId);
                var totalPeriods = g.PeriodWinners.Values.Count(v => v != null);

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
