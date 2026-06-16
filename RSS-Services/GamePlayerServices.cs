using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class GamePlayerServices
    {
        private AppDbContext _appDbContext;
        private readonly SquareServices _squareServices;
        public GamePlayerServices(AppDbContext appDbContext, SquareServices squareServices) 
        { 
           _appDbContext = appDbContext;
            _squareServices = squareServices;
        }

        public GamePlayer CreatePlayerHostedGame(string userId, Guid gameId)
        {

            var gamePlayer = new GamePlayer()
            {
                ApplicationUserId = userId,
                GameId = gameId,
                IsHost = true,
            };
            return gamePlayer;
        }

        public async Task<bool> JoinGame(string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var existing = await _appDbContext.GamePlayers
                .FirstOrDefaultAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);

            if (existing != null) return true;

            var game = await _appDbContext.SquareGames
                .Include(g => g.GamePlayers)
                .FirstOrDefaultAsync(g => g.Id == gameGuid);

            if (game == null)
                throw new ArgumentException($"Game not found: {gameId}");

            if (game.GamePlayers.Count >= game.PlayerCount)
                throw new InvalidOperationException("Game is full.");

            var gamePlayer = new GamePlayer
            {
                ApplicationUserId = userId,
                GameId = gameGuid,
                IsHost = false,
            };

            _appDbContext.GamePlayers.Add(gamePlayer);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AreGamePlayerSelectionsRecorded(int squareSelections, string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var gamePlayer = await _appDbContext.GamePlayers
                .FirstOrDefaultAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);
            if (gamePlayer == null)
            {
                throw new Exception("Game Player not found: " + userId + " for game: " + gameId + "  ");
            }
            var squareGame = await _squareServices.GetSquareGameById(gameId);
            if (squareGame == null)
            {
                throw new Exception($"Square game {gameId} not found.");
            }
            var pricePerSquare = squareGame.PricePerSquare;

            gamePlayer.NumbersOfSquareSelected = squareSelections;
            if (pricePerSquare > 0)
            {
                gamePlayer.TotalWagerAmount = squareSelections * pricePerSquare;
            }
            var saved = await _appDbContext.SaveChangesAsync();

            if (saved > 0) return true;
            return false;
        }
    }
}
