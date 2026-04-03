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
        private readonly Random _random = new();
        public SquareServices(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public List<GameSquares> CreateSquareSelections(List<string> squareSelections, string userId, string gameId)
        {
            var gameSquares = new List<GameSquares>();
            var gameIdGuid = Guid.Parse(gameId);
            var createdAt = DateTimeOffset.UtcNow;

           var gamePlayer = _appDbContext.GamePlayers.FirstOrDefault(g => g.GameId == gameIdGuid && g.ApplicationUserId == userId);

            foreach (var square in squareSelections)
            {
                var squareId = Guid.Parse(square);
                var selectedSquare = _appDbContext.GameSquares.FirstOrDefault(s => s.Id == squareId);
                selectedSquare.GamePlayerId = gamePlayer.Id;

                gameSquares.Add(selectedSquare);
            }
            return gameSquares;
        }

        public List<string> CheckIfSquaresAreSelected(string gameId, List<string> squareSelections)
        {
            var gameIdGuid = Guid.Parse(gameId);
            var unavailableSquares = new List<string>();

            foreach (var square in squareSelections)
            {
                var squareId = Guid.Parse(square);
                var isSquareTaken = _appDbContext.GameSquares
            .Any(gs => gs.Id == squareId &&
                        gs.GamePlayer.GameId == gameIdGuid);

                if (isSquareTaken)
                {
                    unavailableSquares.Add(square);
                }
            }  
            return unavailableSquares;
        }

        public List<GameSquares> GetAllSelectedSquares(string gameId)
        {
            var gameIdGuid = Guid.Parse(gameId);
            return _appDbContext.GameSquares
                 .Include(gs => gs.GamePlayer)
                  .ThenInclude(gp => gp.User)
                 .Where(gs => gs.SquareGamesId == gameIdGuid && gs.GamePlayerId != null)
                 .ToList();
        }

        public async Task GenerateBoardAsync(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            var topNumbers = ShuffleDigits();
            var leftNumbers = ShuffleDigits();

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game is null) return;

            game.TopNumbers = topNumbers;
            game.LeftNumbers = leftNumbers;

            var squares = new List<GameSquares>();

            for (int rowIndex = 0; rowIndex < leftNumbers.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < topNumbers.Count; colIndex++)
                {
                    squares.Add(new GameSquares
                    {
                        SquareGamesId = game.Id,

                        // ✅ Position (for frontend rendering)
                        RowIndex = rowIndex,
                        ColumnIndex = colIndex,

                        // ✅ Digits (for winner logic)
                        AwayDigit = leftNumbers[rowIndex],
                        HomeDigit = topNumbers[colIndex]
                    });
                }
            }

            await _appDbContext.GameSquares.AddRangeAsync(squares);
        }

        private List<int> ShuffleDigits()
        {
            var numbers = Enumerable.Range(0, 10).ToList();

            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);

                // swap
                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }

            return numbers;
        }

        public async Task<List<GameSquares>> GetGameboardSquaresByGameId(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);

            return await _appDbContext.GameSquares
                .Where(sq => sq.SquareGamesId == gameGuid)
                .ToListAsync();
        }

        public async Task<SquareGames?> GetOutsideSquareNumbers(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            return await _appDbContext.SquareGames
                .FirstOrDefaultAsync(x => x.Id == gameGuid);
        }
       

       }
    }

