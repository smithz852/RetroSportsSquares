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

        public async Task AdvanceTurn(string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid)) return;

            var game = await _appDbContext.SquareGames.FindAsync(gameGuid);
            if (game == null || !game.SelectionPhaseActive) return;

            var players = await _appDbContext.GamePlayers
                .Where(p => p.GameId == gameGuid)
                .OrderBy(p => p.TurnOrder)
                .ToListAsync();

            var current = players.FirstOrDefault(p => p.ApplicationUserId == game.CurrentTurnUserId);
            if (current != null) current.HasHadTurn = true;

            var next = players.FirstOrDefault(p => !p.HasHadTurn);
            if (next == null)
            {
                game.SelectionPhaseActive = false;
                game.CurrentTurnUserId = null;
                game.TurnStartedAt = null;
            }
            else
            {
                game.CurrentTurnUserId = next.ApplicationUserId;
                game.TurnStartedAt = DateTimeOffset.UtcNow;
            }

            await _appDbContext.SaveChangesAsync();
            await _hubNotifier.NotifyTurnAdvanced(gameId);
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
