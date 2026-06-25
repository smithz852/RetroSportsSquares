using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;

namespace RSS_Services
{
    public class AvailableGamesServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly TimeHelpers _timeHelpers;

        public AvailableGamesServices(AppDbContext appDbContext, TimeHelpers timeHelpers)
        {
            _appDbContext = appDbContext;
            _timeHelpers = timeHelpers;
        }

        public List<SquareGames> GetAllAvailableGames()
        {
            return _appDbContext.SquareGames
                .Include(g => g.DailySportGame)
                .Include(g => g.GamePlayers)
                .Where(g => g.IsPublic && g.isOpen)
                .ToList();
        }

        public SquareGames CreateGame(string name, bool isOpen, int playerCount, string gameType, int pricePerSquare, int squareSelectionLimit, bool isTurnBased, int turnTimeoutSeconds, string dailySportsGameId, bool isPublic = true)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var createdAt = DateTimeOffset.UtcNow;
            var dailySportGame = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid);

            var periodCount = dailySportGame?.SportType switch
            {
                "soccer" => 2,
                _ => 4
            };

            var game = new SquareGames
            {
                GameName = name,
                isOpen = isOpen,
                PlayerCount = playerCount,
                CreatedAt = createdAt,
                GameType = gameType,
                PricePerSquare = pricePerSquare,
                SquareSelectionLimit = squareSelectionLimit,
                IsTurnBased = isTurnBased,
                TurnTimeoutSeconds = turnTimeoutSeconds,
                DailySportGame = dailySportGame,
                PeriodCount = periodCount,
                IsPublic = isPublic,
            };

            return game;
        }

        public async Task<bool> DeleteGame(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);

            var game = await _appDbContext.SquareGames
                .FirstOrDefaultAsync(g => g.Id == gameGuid);

            if (game == null) return false;

            var squares = _appDbContext.GameSquares.Where(s => s.SquareGamesId == gameGuid);
            _appDbContext.GameSquares.RemoveRange(squares);

            var players = _appDbContext.GamePlayers.Where(p => p.GameId == gameGuid);
            _appDbContext.GamePlayers.RemoveRange(players);

            _appDbContext.SquareGames.Remove(game);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<SquareGames> GetGameById(string id)
        {
            var gameId = Guid.Parse(id);
            return _appDbContext.SquareGames
                .Include(g => g.DailySportGame)
                .Include(g => g.GamePlayers)
                .FirstOrDefault(g => g.Id == gameId);
        }

        public SquareGames GetAllScoreAndWinnerDataByGameId(string id)
        {
            var gameId = Guid.Parse(id);
            return _appDbContext.SquareGames
                .Include(g => g.DailySportGame)
                .FirstOrDefault(g => g.Id == gameId);
        }

        public async Task<Dictionary<int, string?>> GetPeriodWinnerDisplayNames(Dictionary<int, string?> periodWinners)
        {
            if (periodWinners.Count == 0) return new();

            var userIds = periodWinners.Values
                .Where(v => v != null)
                .Cast<string>()
                .Distinct()
                .ToList();

            var users = await _appDbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName);

            return periodWinners.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value != null ? users.GetValueOrDefault(kvp.Value) : null
            );
        }

    }
}