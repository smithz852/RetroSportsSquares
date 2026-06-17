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
                .ToList();
        }

        public SquareGames CreateGame(string name, bool isOpen, int playerCount, string gameType, int pricePerSquare, int squareSelectionLimit, bool isTurnBased, int turnTimeoutSeconds, string dailySportsGameId)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var createdAt = DateTimeOffset.UtcNow;

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
                DailySportGame = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid)
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
                .Include(g => g.WinnerQ1)
                .Include(g => g.WinnerQ2)
                .Include(g => g.WinnerQ3)
                .Include(g => g.WinnerQ4)
                .FirstOrDefault(g => g.Id == gameId);
        }

    }
}