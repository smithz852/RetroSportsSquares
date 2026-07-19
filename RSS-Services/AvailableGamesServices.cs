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
        private readonly WalletService _walletService;

        public AvailableGamesServices(AppDbContext appDbContext, TimeHelpers timeHelpers, WalletService walletService)
        {
            _appDbContext = appDbContext;
            _timeHelpers = timeHelpers;
            _walletService = walletService;
        }

        public static int GetPeriodCountForSport(string? sportType) => sportType switch
        {
            "soccer" => 2,
            _ => 4
        };

        public List<SquareGames> GetAllAvailableGames()
        {
            return _appDbContext.SquareGames
                .Include(g => g.DailySportGame)
                .Include(g => g.GamePlayers)
                .Where(g => g.IsPublic && g.isOpen)
                .ToList();
        }

        public SquareGames CreateGame(string name, bool isOpen, int playerCount, string gameType, decimal pricePerSquare, int squareSelectionLimit, bool isTurnBased, int turnTimeoutSeconds, string dailySportsGameId, bool isPublic = true, string payoutMode = PayoutModes.Default)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var createdAt = DateTimeOffset.UtcNow;
            var dailySportGame = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid);

            var periodCount = GetPeriodCountForSport(dailySportGame?.SportType);

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
                PayoutMode = payoutMode,
            };

            return game;
        }

        public async Task<bool> DeleteGame(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);

            var game = await _appDbContext.SquareGames
                .FirstOrDefaultAsync(g => g.Id == gameGuid);

            if (game == null) return false;

            await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                // Deletion is only reachable while the game is open (pre-start), so a
                // refund here can never collide with end-of-game settlement.
                if (game.IsPublic)
                    await _walletService.RefundGameWagersAsync(gameGuid);

                var squares = _appDbContext.GameSquares.Where(s => s.SquareGamesId == gameGuid);
                _appDbContext.GameSquares.RemoveRange(squares);

                var players = _appDbContext.GamePlayers.Where(p => p.GameId == gameGuid);
                _appDbContext.GamePlayers.RemoveRange(players);

                var chatMessages = _appDbContext.ChatMessages.Where(m => m.GameId == gameGuid);
                _appDbContext.ChatMessages.RemoveRange(chatMessages);

                _appDbContext.SquareGames.Remove(game);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

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
                .Include(g => g.GameSquares)
                .FirstOrDefault(g => g.Id == gameId);
        }

        public decimal GetPayoutPerPeriod(SquareGames game)
        {
            var claimedSquares = game.GameSquares.Count(gs => gs.GamePlayerId != null);
            return PayoutCalculator.GetPayoutPerPeriod(game.PricePerSquare, claimedSquares, game.PeriodCount);
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