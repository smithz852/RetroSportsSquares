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

        public async Task<List<GameSquares>> CreateSquareSelections(List<string> squareSelections, string userId, string gameId)
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
            var savedSquares = await _appDbContext.SaveChangesAsync();
            if (savedSquares <= 0)
            {
                return null;
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

            var alreadyGenerated = await _appDbContext.GameSquares.AnyAsync(s => s.SquareGamesId == gameGuid);
            if (alreadyGenerated) return;

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
                .Include(sq => sq.GamePlayer)
                 .ThenInclude(gp => gp.User)
                .Where(sq => sq.SquareGamesId == gameGuid)
                .ToListAsync();
        }

        public async Task<SquareGames?> GetOutsideSquareNumbers(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            return await _appDbContext.SquareGames
                .FirstOrDefaultAsync(x => x.Id == gameGuid);
        }
       
        public async Task<QuarterlyWinnerDTO> DetermineQuarterlyWinner(SportScoreUpdateDTO newScore, Guid gameId)
        {
            var currentQuarter = GetCurrentGamePeriodIndex(newScore.Status);
            var completedQuarter = currentQuarter - 1;
            if (completedQuarter < 1)
            {
                return null;
            }
            var winningHomeDigit = newScore.CurrentHomeScore % 10;
            var winningAwayDigit = newScore.CurrentAwayScore % 10;

            var winningSquare = await _appDbContext.GameSquares
                .Include(gs => gs.GamePlayer)
                .ThenInclude(gp => gp.User)
                .FirstOrDefaultAsync(sq =>
                    sq.SquareGamesId == gameId &&
                    sq.HomeDigit == winningHomeDigit &&
                    sq.AwayDigit == winningAwayDigit &&
                    sq.GamePlayerId != null);

            var quarterlyWinner = new QuarterlyWinnerDTO
            {
                Period = completedQuarter,
                UserId = winningSquare.GamePlayerId
            };
            return quarterlyWinner;
        }

        public async Task SaveQuarterlyWinner(QuarterlyWinnerDTO winner, Guid squareGameId)
        {
            var game = await _appDbContext.SquareGames.FindAsync(squareGameId);
            var player = await _appDbContext.GamePlayers.FindAsync(winner.UserId);
            if (player is null) throw new InvalidOperationException($"Player {winner.UserId} not found");
            if (game is null) throw new InvalidOperationException($"Game {squareGameId} not found");

            var periodSetters = new Dictionary<int, Action<SquareGames, string>>
            {
                { 1, (g, id) => g.WinnerQ1Id = id },
                { 2, (g, id) => g.WinnerQ2Id = id },
                { 3, (g, id) => g.WinnerQ3Id = id },
                { 4, (g, id) => g.WinnerQ4Id = id },
            };

            if (periodSetters.TryGetValue(winner.Period, out var setter))
                setter(game, player.ApplicationUserId);

           var saved = await _appDbContext.SaveChangesAsync();
            if (saved <= 0) throw new InvalidOperationException("Could not save winner");
        }

        private static readonly Dictionary<string, int> PeriodMap = new()
        {
            { "Q1", 1 }, { "Q2", 2 }, { "HALF", 3 },
            { "Q3", 3 }, { "Q4", 4 },
            { "FINAL", 5 }, { "FT", 5 }, { "OT", 5 }
        };

        public static int GetCurrentGamePeriodIndex(string? period)
        {
            if (string.IsNullOrEmpty(period)) return 0;
            var upper = period.ToUpper();
            foreach (var key in PeriodMap.Keys)
                if (upper.Contains(key)) return PeriodMap[key];
            return 0;
        }

        public async Task<bool> SetGameToClosedById(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null) return false;
            game.isOpen = false;
           var savedStatusChange = await _appDbContext.SaveChangesAsync();
            if (savedStatusChange <= 0)
            {
                return false;
            }
            return true;
        }

        //public async Task<Dictionary<int, string?>> GetQuarterWinners(Guid gameId)
        //{
        //    var game = await _appDbContext.SquareGames
        //        .Include(g => g.WinnerQ1)
        //        .Include(g => g.WinnerQ2)
        //        .Include(g => g.WinnerQ3)
        //        .Include(g => g.WinnerQ4)
        //        .FirstOrDefaultAsync(g => g.Id == gameId);

        //    if (game is null) throw new InvalidOperationException($"Game {gameId} not found");

        //    return new Dictionary<int, string?>
        //    {
        //        { 1, game.WinnerQ1?.UserName },
        //        { 2, game.WinnerQ2?.UserName },
        //        { 3, game.WinnerQ3?.UserName },
        //        { 4, game.WinnerQ4?.UserName },
        //    };
        //}

    }
    }

