using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSS_DB.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? GamerTag { get; set; }
        // Only ever modified via atomic ExecuteUpdate arithmetic (see WalletService) —
        // never through tracked-entity writes, so concurrent grant/wager can't clobber it.
        [Column(TypeName = "decimal(18,2)")]
        public decimal CoinBalance { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    }
}