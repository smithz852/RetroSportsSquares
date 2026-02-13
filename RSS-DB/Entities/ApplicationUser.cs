using Microsoft.AspNetCore.Identity;

namespace RSS_DB.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    }
}