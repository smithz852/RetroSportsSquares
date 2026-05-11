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

        public async Task<bool> AreGamePlayerSelectionsRecorded(int squareSelections, string userId, string gameId)
        {
            var gamePlayer = await _appDbContext.GamePlayers.FirstOrDefaultAsync(u => u.ApplicationUserId == userId);
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
