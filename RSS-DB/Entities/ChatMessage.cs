using System;
using System.ComponentModel.DataAnnotations;

namespace RSS_DB.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        [Required]
        public Guid GameId { get; set; }
        public SquareGames Game { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }

        public ChatMessage()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
