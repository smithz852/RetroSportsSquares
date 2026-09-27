using Microsoft.EntityFrameworkCore;
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
    public class GamePlayerServices
    {
        private AppDbContext _appDbContext;
        private readonly SquareServices _squareServices;
        private readonly IGameHubNotifier _hubNotifier;

        public GamePlayerServices(AppDbContext appDbContext, SquareServices squareServices, IGameHubNotifier hubNotifier)
        {
            _appDbContext = appDbContext;
            _squareServices = squareServices;
            _hubNotifier = hubNotifier;
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

        public async Task<GamePlayer> JoinGame(string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var existing = await _appDbContext.GamePlayers
                .FirstOrDefaultAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);

            if (existing != null) return existing;

            var game = await _appDbContext.SquareGames
                .Include(g => g.GamePlayers)
                .FirstOrDefaultAsync(g => g.Id == gameGuid);

            if (game == null)
                throw new ArgumentException($"Game not found: {gameId}");

            // Existing members returned early above, so this only blocks new joiners.
            if (!game.isOpen || game.IsCompleted)
                throw new InvalidOperationException("This game has already started.");

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
            await _hubNotifier.NotifyPlayerJoined(gameId);
            return gamePlayer;
        }

        public async Task<bool> IsPlayerInGame(string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                return false;

            return await _appDbContext.GamePlayers
                .AnyAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);
        }

        public async Task<bool> IsPlayerHost(string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                return false;

            var gamePlayer = await _appDbContext.GamePlayers
                .FirstOrDefaultAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);

            return gamePlayer?.IsHost ?? false;
        }

        public async Task BeginSelections(string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null) throw new ArgumentException("Game not found");
            if (!game.IsTurnBased) throw new InvalidOperationException("Game is not turn-based.");
            if (game.SelectionPhaseActive) throw new InvalidOperationException("Selection phase has already begun.");

            var players = await _appDbContext.GamePlayers
                .Where(p => p.GameId == gameGuid)
                .ToListAsync();

            if (players.Count == 0) throw new InvalidOperationException("No players have joined yet.");

            var shuffled = players.OrderBy(_ => Guid.NewGuid()).ToList();
            for (int i = 0; i < shuffled.Count; i++)
                shuffled[i].TurnOrder = i + 1;

            game.SelectionPhaseActive = true;
            game.CurrentTurnUserId = shuffled[0].ApplicationUserId;
            game.TurnStartedAt = DateTimeOffset.UtcNow;

            await _appDbContext.SaveChangesAsync();
            await _hubNotifier.NotifySelectionsStarted(gameId);
        }

        // Standalone advance (host skip): takes the same per-game lock as square selection so a skip
        // and a submit can't interleave, then notifies once the change is committed.
        public async Task<bool> AdvanceTurn(string gameId, string? expectedCurrentUserId = null)
        {
            if (!Guid.TryParse(gameId, out var gameGuid)) return false;

            await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            await _squareServices.LockGameForUpdateAsync(gameGuid);

            if (!await TryAdvanceTurnAsync(gameGuid, expectedCurrentUserId))
                return false; // nothing changed; disposing the transaction rolls it back

            await transaction.CommitAsync();
            await _hubNotifier.NotifyTurnAdvanced(gameId);
            return true;
        }

        // Core advance — runs inside the caller's transaction (which should hold the game lock) and
        // does not notify; the caller notifies after commit. expectedCurrentUserId makes it
        // conditional: it only proceeds if that player still holds the turn. Returns false when the
        // turn had already moved (or the selection phase isn't active).
        public async Task<bool> TryAdvanceTurnAsync(Guid gameGuid, string? expectedCurrentUserId = null)
        {
            // Untracked reads — the scoped context may hold stale copies from earlier in the request.
            var game = await _appDbContext.SquareGames.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameGuid);
            if (game == null || !game.SelectionPhaseActive) return false;

            var currentUserId = game.CurrentTurnUserId;
            if (expectedCurrentUserId != null && expectedCurrentUserId != currentUserId) return false;

            var players = await _appDbContext.GamePlayers
                .AsNoTracking()
                .Where(p => p.GameId == gameGuid)
                .OrderBy(p => p.TurnOrder)
                .ToListAsync();

            var next = players.FirstOrDefault(p => !p.HasHadTurn && p.ApplicationUserId != currentUserId);

            // Belt and braces on top of the game lock: the WHERE on CurrentTurnUserId means a
            // caller without the lock still can't move the turn off a player twice.
            var claimGame = _appDbContext.SquareGames
                .Where(g => g.Id == gameGuid && g.SelectionPhaseActive && g.CurrentTurnUserId == currentUserId);

            var claimed = next == null
                ? await claimGame.ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.SelectionPhaseActive, false)
                    .SetProperty(g => g.CurrentTurnUserId, (string?)null)
                    .SetProperty(g => g.TurnStartedAt, (DateTimeOffset?)null))
                : await claimGame.ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.CurrentTurnUserId, next.ApplicationUserId)
                    .SetProperty(g => g.TurnStartedAt, (DateTimeOffset?)DateTimeOffset.UtcNow));

            if (claimed == 0) return false;

            await _appDbContext.GamePlayers
                .Where(p => p.GameId == gameGuid && p.ApplicationUserId == currentUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.HasHadTurn, true));

            return true;
        }

        public async Task LeaveGame(string userId, string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var gamePlayer = await _appDbContext.GamePlayers
                .FirstOrDefaultAsync(gp => gp.ApplicationUserId == userId && gp.GameId == gameGuid);

            if (gamePlayer == null)
                throw new ArgumentException("Player is not in this game.");

            if (gamePlayer.IsHost)
                throw new InvalidOperationException("The host cannot leave. Delete the game instead.");

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null)
                throw new ArgumentException("Game not found.");

            if (!game.isOpen || game.SelectionPhaseActive || game.IsCompleted)
                throw new InvalidOperationException("Cannot leave after the game has started.");

            _appDbContext.GamePlayers.Remove(gamePlayer);
            await _appDbContext.SaveChangesAsync();
            await _hubNotifier.NotifyPlayerLeft(gameId);
        }

        public async Task<TurnStatusDTO> GetTurnStatus(string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null) throw new ArgumentException("Game not found");

            var players = await _appDbContext.GamePlayers
                .Include(p => p.User)
                .Where(p => p.GameId == gameGuid)
                .OrderBy(p => p.TurnOrder)
                .ToListAsync();

            return new TurnStatusDTO
            {
                SelectionPhaseActive = game.SelectionPhaseActive,
                CurrentTurnUserId = game.CurrentTurnUserId,
                TurnStartedAt = game.TurnStartedAt,
                TurnTimeoutSeconds = game.TurnTimeoutSeconds,
                Players = players.Select(p => new TurnPlayerDTO
                {
                    UserId = p.ApplicationUserId,
                    DisplayName = p.User?.DisplayName ?? "Unknown",
                    TurnOrder = p.TurnOrder,
                    HasHadTurn = p.HasHadTurn,
                    IsHost = p.IsHost,
                }).ToList()
            };
        }

    }
}
