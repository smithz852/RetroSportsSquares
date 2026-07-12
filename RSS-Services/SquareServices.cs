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
        private readonly IGameHubNotifier _hubNotifier;

        public SquareServices(AppDbContext appDbContext, IGameHubNotifier hubNotifier)
        {
            _appDbContext = appDbContext;
            _hubNotifier = hubNotifier;
        }

        public async Task<List<GameSquares>> CreateSquareSelections(List<string> squareSelections, string userId, string gameId)
        {
            var gameSquares = new List<GameSquares>();
            var gameIdGuid = Guid.Parse(gameId);
            var createdAt = DateTimeOffset.UtcNow;

           var gamePlayer = _appDbContext.GamePlayers.FirstOrDefault(g => g.GameId == gameIdGuid && g.ApplicationUserId == userId);

            var availableSquares = CheckIfSquaresAreSelected(gameId, squareSelections);
            
            foreach (var square in availableSquares)
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

            if (availableSquares.Count < squareSelections.Count)
                throw new InvalidOperationException($"Some squares aren't available, please choose {squareSelections.Count - availableSquares.Count} more squares.");

            return gameSquares;
        }

        public async Task<bool> SquareLimitCheck(string gameId, string userId, int incomingCount)
        {
            var gameGuid = Guid.Parse(gameId);

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null || game.SquareSelectionLimit <= 0) return true;

            var alreadySelected = await _appDbContext.GameSquares
                .Where(sq => sq.SquareGamesId == gameGuid && sq.GamePlayer.ApplicationUserId == userId)
                .CountAsync();

            return (alreadySelected + incomingCount) <= game.SquareSelectionLimit;
        }

        public async Task<int> GetSquareSelectionLimit(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            var game =  _appDbContext.SquareGames.FirstOrDefault(g => g.Id == gameGuid);
            if (game is null) throw new InvalidOperationException($"Game {gameId} not found");
            return game.SquareSelectionLimit;
        }

        public List<string> CheckIfSquaresAreSelected(string gameId, List<string> squareSelections)
        {
            var gameIdGuid = Guid.Parse(gameId);
            var availableSquares = new List<string>();

            foreach (var square in squareSelections)
            {
                var squareId = Guid.Parse(square);
                var isSquareTaken = _appDbContext.GameSquares
            .Any(gs => gs.Id == squareId &&
                        gs.GamePlayer.GameId == gameIdGuid);

                if (!isSquareTaken)
                {
                    availableSquares.Add(square);
                }
            }  
            return availableSquares;
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

        public async Task<SquareGames> GetSquareGameById(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            return await _appDbContext.SquareGames
                .FirstOrDefaultAsync(g => g.Id == gameGuid);
        }

        public async Task<SquareGames?> GetOutsideSquareNumbers(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            return await _appDbContext.SquareGames
                .FirstOrDefaultAsync(x => x.Id == gameGuid);
        }
       
        public async Task<QuarterlyWinnerDTO> DetermineQuarterlyWinner(SportScoreUpdateDTO newScore, Guid gameId)
        {
            var currentPeriod = GetCurrentGamePeriodIndex(newScore.Status, newScore.SportType);
            var completedPeriod = currentPeriod - 1;
            if (completedPeriod < 1 || completedPeriod > newScore.HomePeriodScores.Count)
                return null;

            var homeTotal = newScore.HomePeriodScores.Take(completedPeriod).Sum();
            var awayTotal = newScore.AwayPeriodScores.Take(completedPeriod).Sum();

            var winningHomeDigit = homeTotal % 10;
            var winningAwayDigit = awayTotal % 10;

            var winningSquare = await _appDbContext.GameSquares
                .FirstOrDefaultAsync(sq =>
                    sq.SquareGamesId == gameId &&
                    sq.HomeDigit == winningHomeDigit &&
                    sq.AwayDigit == winningAwayDigit &&
                    sq.GamePlayerId != null);

            return new QuarterlyWinnerDTO
            {
                Period = completedPeriod,
                UserId = winningSquare?.GamePlayerId
            };
        }

        public async Task<QuarterlyWinnerSaveResult?> SaveQuarterlyWinner(QuarterlyWinnerDTO winner, Guid squareGameId)
        {
            var game = await _appDbContext.SquareGames.FindAsync(squareGameId);
            if (game is null) throw new InvalidOperationException($"Game {squareGameId} not found");

            var player = await _appDbContext.GamePlayers.FindAsync(winner.UserId);
            if (player is null) throw new InvalidOperationException($"Player {winner.UserId} not found");

            // Already resolved — skip
            if (game.PeriodWinners.ContainsKey(winner.Period)) return null;

            // Backfill any periods that were skipped (no winning square)
            for (int p = 1; p < winner.Period; p++)
            {
                if (!game.PeriodWinners.ContainsKey(p))
                    game.PeriodWinners[p] = null;
            }

            game.PeriodWinners[winner.Period] = player.ApplicationUserId;

            if (winner.Period >= game.PeriodCount)
                game.IsCompleted = true;

            // Notify EF Core the JSON column was mutated
            _appDbContext.Entry(game).Property(g => g.PeriodWinners).IsModified = true;

            var saved = await _appDbContext.SaveChangesAsync();
            if (saved <= 0) throw new InvalidOperationException("Could not save winner");

            return new QuarterlyWinnerSaveResult
            {
                SquareGameId = game.Id,
                Period = winner.Period,
                WinnerApplicationUserId = player.ApplicationUserId,
                GameCompleted = game.IsCompleted
            };
        }

        private static readonly Dictionary<string, Dictionary<string, int>> SportPeriodMaps = new()
        {
            ["basketball"] = new()
            {
                ["Q1"] = 1, ["Q2"] = 2, ["HALF"] = 3, ["HT"] = 3,
                ["Q3"] = 3, ["Q4"] = 4,
                ["FINAL"] = 5, ["FT"] = 5, ["OT"] = 5, ["AOT"] = 5
            },
            ["american-football"] = new()
            {
                ["Q1"] = 1, ["Q2"] = 2, ["HALF"] = 3, ["HT"] = 3,
                ["Q3"] = 3, ["Q4"] = 4,
                ["FINAL"] = 5, ["FT"] = 5, ["OT"] = 5, ["AOT"] = 5
            },
            ["soccer"] = new()
            {
                ["1H"] = 1, ["HT"] = 2, ["2H"] = 2,
                ["ET"] = 2, ["BT"] = 2, ["P"] = 2,
                ["FT"] = 3, ["AET"] = 3, ["PEN"] = 3
            },
        };

        public static int GetCurrentGamePeriodIndex(string? status, string? sportType)
        {
            if (string.IsNullOrEmpty(status)) return 0;
            var mapKey = sportType?.ToLower() ?? "basketball";
            if (!SportPeriodMaps.TryGetValue(mapKey, out var periodMap)) return 0;
            var upper = status.ToUpper();
            foreach (var key in periodMap.Keys)
                if (upper.Contains(key)) return periodMap[key];
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

        public async Task<List<SquareGames>> GetSquareGamesBySportsGameId(Guid sportsGameId)
        {
            return await _appDbContext.SquareGames
                .Where(g => g.DailySportGameId == sportsGameId)
                .ToListAsync();
        }

        public async Task NotifyScoreUpdatedAsync(Guid sportsGameId)
        {
            var squareGames = await GetSquareGamesBySportsGameId(sportsGameId);
            foreach (var squareGame in squareGames)
                await _hubNotifier.NotifyScoreUpdated(squareGame.Id.ToString());
        }

        // Fallback for games left dangling by a missed/skipped quarterly-winner check (e.g. server downtime
        // spanning midnight PST). Closes games the host never started, and force-completes games that started
        // but never reached IsCompleted, backfilling any unresolved periods as null so past-game stats stay consistent.
        public async Task<List<Guid>> CloseStaleGamesAsync(string sportType, DateTime todayPst)
        {
            var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

            var incompleteGames = await _appDbContext.SquareGames
                .Include(g => g.DailySportGame)
                .Where(g => g.DailySportGame.SportType == sportType && !g.IsCompleted)
                .ToListAsync();

            var staleGames = incompleteGames
                .Where(g => TimeZoneInfo.ConvertTime(g.DailySportGame.GameStartTime, pacific).Date < todayPst)
                .ToList();

            var completedStartedGameIds = new List<Guid>();
            if (staleGames.Count == 0) return completedStartedGameIds;

            foreach (var game in staleGames)
            {
                if (!game.isOpen)
                {
                    for (int period = 1; period <= game.PeriodCount; period++)
                    {
                        if (!game.PeriodWinners.ContainsKey(period))
                            game.PeriodWinners[period] = null;
                    }
                    _appDbContext.Entry(game).Property(g => g.PeriodWinners).IsModified = true;

                    // Only games that actually ran get a recap email; games the host
                    // never started are just closed silently.
                    completedStartedGameIds.Add(game.Id);
                }

                game.isOpen = false;
                game.IsCompleted = true;
            }

            await _appDbContext.SaveChangesAsync();
            return completedStartedGameIds;
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

