using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.DTOs;

namespace RSS_Services
{
    public class ChatServices
    {
        private const int MaxMessageLength = 500;
        private const int RecentMessageCount = 100;

        private readonly AppDbContext _appDbContext;
        private readonly IGameHubNotifier _hubNotifier;

        public ChatServices(AppDbContext appDbContext, IGameHubNotifier hubNotifier)
        {
            _appDbContext = appDbContext;
            _hubNotifier = hubNotifier;
        }

        public async Task<ChatMessageDTO> SendMessage(string userId, string gameId, string message)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            message = message?.Trim() ?? "";
            if (message.Length == 0)
                throw new ArgumentException("Message cannot be empty.");
            if (message.Length > MaxMessageLength)
                throw new ArgumentException($"Message cannot exceed {MaxMessageLength} characters.");

            var isPlayer = await _appDbContext.GamePlayers
                .AnyAsync(gp => gp.GameId == gameGuid && gp.ApplicationUserId == userId);
            if (!isPlayer)
                throw new InvalidOperationException("You must be a player in this game to chat.");

            var chatMessage = new ChatMessage
            {
                GameId = gameGuid,
                ApplicationUserId = userId,
                Message = message,
            };

            _appDbContext.ChatMessages.Add(chatMessage);
            await _appDbContext.SaveChangesAsync();

            var user = await _appDbContext.Users.FindAsync(userId);
            var dto = new ChatMessageDTO
            {
                Id = chatMessage.Id,
                UserId = userId,
                DisplayName = user?.GamerTag ?? user?.DisplayName ?? "Unknown",
                Message = chatMessage.Message,
                CreatedAt = chatMessage.CreatedAt,
                IsDeleted = false,
            };

            await _hubNotifier.NotifyChatMessage(gameId, dto);
            return dto;
        }

        public async Task<List<ChatMessageDTO>> GetRecentMessages(string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            var messages = await _appDbContext.ChatMessages
                .Where(m => m.GameId == gameGuid && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(RecentMessageCount)
                .Select(m => new ChatMessageDTO
                {
                    Id = m.Id,
                    UserId = m.ApplicationUserId,
                    DisplayName = m.User.GamerTag ?? m.User.DisplayName ?? "Unknown",
                    Message = m.Message,
                    CreatedAt = m.CreatedAt,
                    IsDeleted = false,
                })
                .ToListAsync();

            messages.Reverse();
            return messages;
        }

        public async Task<List<ChatMessageDTO>> GetChatLogForGame(string gameId)
        {
            if (!Guid.TryParse(gameId, out var gameGuid))
                throw new ArgumentException($"Invalid game ID: {gameId}");

            return await _appDbContext.ChatMessages
                .Where(m => m.GameId == gameGuid)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .Select(m => new ChatMessageDTO
                {
                    Id = m.Id,
                    UserId = m.ApplicationUserId,
                    DisplayName = m.User.GamerTag ?? m.User.DisplayName ?? "Unknown",
                    Message = m.Message,
                    CreatedAt = m.CreatedAt,
                    IsDeleted = m.IsDeleted,
                })
                .ToListAsync();
        }
    }
}
