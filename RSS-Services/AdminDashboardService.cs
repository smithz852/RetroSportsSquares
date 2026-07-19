using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_Services.DTOs;
using RSS_Services.Helpers;

namespace RSS_Services
{
    public class AdminDashboardService
    {
        private const string AdminRoleName = "Admin";
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminSummaryDTO> GetSummaryAsync()
        {
            return new AdminSummaryDTO
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveGames = await _context.SquareGames.CountAsync(sg => !sg.IsCompleted),
                CompletedGames = await _context.SquareGames.CountAsync(sg => sg.IsCompleted),
                TotalCoinsWagered = await _context.GameSquares
                    .Where(gs => gs.GamePlayerId != null)
                    .SumAsync(gs => gs.SquareGames.PricePerSquare)
            };
        }

        public async Task<List<AdminCurrentGameDTO>> GetCurrentGamesAsync()
        {
            return await _context.SquareGames
                .Where(sg => !sg.IsCompleted)
                .OrderByDescending(sg => sg.CreatedAt)
                .Select(sg => new AdminCurrentGameDTO
                {
                    GameId = sg.Id,
                    GameName = sg.GameName,
                    GameType = sg.GameType,
                    League = sg.DailySportGame.League,
                    Matchup = sg.DailySportGame.AwayTeam + " @ " + sg.DailySportGame.HomeTeam,
                    HostDisplayName = sg.GamePlayers
                        .Where(gp => gp.IsHost)
                        .Select(gp => gp.User.DisplayName)
                        .FirstOrDefault(),
                    PlayersJoined = sg.GamePlayers.Count(),
                    MaxPlayers = sg.PlayerCount,
                    PricePerSquare = sg.PricePerSquare,
                    SquaresClaimed = sg.GameSquares.Count(gs => gs.GamePlayerId != null),
                    IsOpen = sg.isOpen,
                    SelectionPhaseActive = sg.SelectionPhaseActive,
                    IsPublic = sg.IsPublic,
                    CreatedAt = sg.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AdminPaginatedPastGamesDTO> GetPastGamesAsync(int page, int pageSize)
        {
            var query = _context.SquareGames.Where(sg => sg.IsCompleted);

            var totalCount = await query.CountAsync();

            var rawGames = await query
                .OrderByDescending(sg => sg.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sg => new
                {
                    sg.Id,
                    sg.GameName,
                    sg.GameType,
                    sg.DailySportGame.League,
                    sg.DailySportGame.HomeTeam,
                    sg.DailySportGame.AwayTeam,
                    HostDisplayName = sg.GamePlayers
                        .Where(gp => gp.IsHost)
                        .Select(gp => gp.User.DisplayName)
                        .FirstOrDefault(),
                    PlayersJoined = sg.GamePlayers.Count(),
                    sg.PricePerSquare,
                    sg.CreatedAt,
                    sg.PeriodWinners,
                    ClaimedSquaresCount = sg.GameSquares.Count(gs => gs.GamePlayerId != null)
                })
                .ToListAsync();

            // PeriodWinners stores user IDs; resolve them to display names in one lookup
            var winnerIds = rawGames
                .SelectMany(g => g.PeriodWinners.Values)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var winnerNames = await _context.Users
                .Where(u => winnerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName);

            var games = rawGames.Select(g => new AdminPastGameDTO
            {
                GameId = g.Id,
                GameName = g.GameName,
                GameType = g.GameType,
                League = g.League,
                Matchup = g.AwayTeam + " @ " + g.HomeTeam,
                HostDisplayName = g.HostDisplayName,
                PlayersJoined = g.PlayersJoined,
                PricePerSquare = g.PricePerSquare,
                TotalPot = g.PricePerSquare * g.ClaimedSquaresCount,
                PeriodWinners = g.PeriodWinners
                    .OrderBy(pw => pw.Key)
                    .Select(pw => new AdminPeriodWinnerDTO
                    {
                        Period = pw.Key,
                        WinnerDisplayName = pw.Value != null && winnerNames.ContainsKey(pw.Value)
                            ? winnerNames[pw.Value]
                            : null
                    })
                    .ToList(),
                CreatedAt = g.CreatedAt
            }).ToList();

            return new AdminPaginatedPastGamesDTO
            {
                Games = games,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<AdminPlayerStatsDTO>> GetPlayerStatsAsync()
        {
            var gameData = await _context.GamePlayers
                .Select(gp => new
                {
                    gp.ApplicationUserId,
                    gp.User.DisplayName,
                    gp.User.GamerTag,
                    gp.Game.PricePerSquare,
                    gp.Game.PeriodWinners,
                    gp.Game.PeriodCount,
                    gp.Game.PayoutMode,
                    gp.Game.IsCompleted,
                    PlayerSquaresCount = gp.GamePlayerSquares.Count(),
                    TotalGameSquaresCount = gp.Game.GameSquares.Count(gs => gs.GamePlayerId != null),
                    ClaimedSquareOwners = gp.Game.GameSquares
                        .Where(gs => gs.GamePlayerId != null)
                        .Select(gs => gs.GamePlayer.ApplicationUserId)
                        .ToList()
                })
                .ToListAsync();

            return gameData
                .GroupBy(g => g.ApplicationUserId)
                .Select(group =>
                {
                    var userId = group.Key;
                    var periodsWon = group.Sum(g => g.PeriodWinners.Values.Count(v => v == userId));
                    var totalPeriodsPlayed = group.Sum(g => g.PeriodWinners.Values.Count(v => v != null));

                    // Same convention as the player dashboard: completed games settle
                    // through the engine (mode-correct), in-progress games count the
                    // flat per-period share as a floor estimate.
                    var wagersWon = group.Sum(g =>
                    {
                        if (g.IsCompleted)
                            return SettlementEngine.GetUserPrizeTotal(g.PayoutMode, g.PeriodWinners, g.PeriodCount, g.PricePerSquare, g.ClaimedSquareOwners, userId);

                        var won = g.PeriodWinners.Values.Count(v => v == userId);
                        return won * PayoutCalculator.GetPayoutPerPeriod(g.PricePerSquare, g.TotalGameSquaresCount, g.PeriodCount);
                    });

                    return new AdminPlayerStatsDTO
                    {
                        UserId = userId,
                        DisplayName = group.First().DisplayName,
                        GamerTag = group.First().GamerTag,
                        GamesPlayed = group.Count(),
                        TotalSquaresClaimed = group.Sum(g => g.PlayerSquaresCount),
                        PeriodsWon = periodsWon,
                        TotalWagered = group.Sum(g => g.PricePerSquare * g.PlayerSquaresCount),
                        WagersWon = wagersWon,
                        WinRate = totalPeriodsPlayed > 0
                            ? Math.Round((double)periodsWon / totalPeriodsPlayed, 3)
                            : 0
                    };
                })
                .OrderByDescending(s => s.TotalWagered)
                .ToList();
        }

        public async Task<List<AdminUserSummaryDTO>> GetUsersAsync()
        {
            var adminUserIds = await _context.UserRoles
                .Where(ur => _context.Roles
                    .Where(r => r.Name == AdminRoleName)
                    .Select(r => r.Id)
                    .Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .ToListAsync();

            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserSummaryDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    GamerTag = u.GamerTag,
                    CreatedAt = u.CreatedAt,
                    GamesPlayed = u.GamePlayers.Count()
                })
                .ToListAsync();

            foreach (var user in users)
                user.IsAdmin = adminUserIds.Contains(user.Id);

            return users;
        }
    }
}
