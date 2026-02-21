using Humanizer;
using Microsoft.EntityFrameworkCore;
using RSS;
using RSS.DTOs;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class SquareServices
    {
        private AppDbContext _appDbContext;
        public SquareServices(AppDbContext appDbContext) 
        { 
            _appDbContext = appDbContext;
        }

        public List<GamePlayerSquare> CreateSquareSelections(List<string> squareSelections, string userId, string gameId)
        {
            var gamePlayerSquares = new List<GamePlayerSquare>();
            var gameIdGuid = Guid.Parse(gameId);
            var createdAt = DateTimeOffset.UtcNow;

           var gamePlayer = _appDbContext.GamePlayers.FirstOrDefault(g => g.GameId == gameIdGuid && g.ApplicationUserId == userId);

            foreach (var square in squareSelections)
            {
                var selectedSquare = _appDbContext.Squares.FirstOrDefault(s => s.Name == square);
                GamePlayerSquare gamePlayerSquare = new GamePlayerSquare()
                {
                    SelectedAt = createdAt,
                    GamePlayerId = gamePlayer.Id,
                    SquaresId = selectedSquare.Id
                };
                gamePlayerSquares.Add(gamePlayerSquare);
            }
            return gamePlayerSquares;
        }

        public List<string> CheckIfSquaresAreSelected(string gameId, List<string> squareSelections)
        {
            var gameIdGuid = Guid.Parse(gameId);
            var unavailableSquares = new List<string>();

            foreach (var square in squareSelections)
            {
                var isSquareTaken = _appDbContext.GamePlayerSquares
            .Any(gps => gps.Squares.Name == square &&
                        gps.GamePlayer.GameId == gameIdGuid);

                if (isSquareTaken)
                {
                    unavailableSquares.Add(square);
                }
            }  
            return unavailableSquares;
        }

        public List<GamePlayerSquare> GetAllSelectedSquares(string gameId)
        {
            var gameIdGuid = Guid.Parse(gameId);
            return _appDbContext.GamePlayerSquares
                 .Include(gps => gps.GamePlayer)
                  .ThenInclude(gp => gp.User)
                 .Include(gps => gps.Squares)
                 .Where(gps => gps.GamePlayer.GameId == gameIdGuid)
                 .ToList();
        }

        public List<GameSquares> SetOutsideGameSquares(string gameId, OutsideSquareNumbersDTO outsideSquares)
        {
            var gameIdGuid = Guid.Parse(gameId);
            var gameSquares = new List<GameSquares>();

            foreach (var item in outsideSquares.OutsideSquares)
            {
                var setSquare = _appDbContext.Squares.FirstOrDefault(s => s.Name == item.SquareName);
                gameSquares.Add(new GameSquares()
                {
                    SquareGamesId = gameIdGuid,
                    SquaresId = setSquare.Id,
                    SquareValue = item.SquareValue
                });
            }

            return gameSquares;
        }

       }
    }

