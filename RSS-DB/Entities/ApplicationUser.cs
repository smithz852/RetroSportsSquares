using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSS_DB.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? GamerTag { get; set; }
        // Only ever modified via atomic ExecuteUpdate arithmetic (see CoinWalletService) —
        // never through tracked-entity writes, so concurrent grant/wager can't clobber it.
        [Column(TypeName = "decimal(18,2)")]
        public decimal CoinBalance { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
        public bool HasSeenWelcome { get; set; } = false;
        // Marks a disposable "tombstone" account created by account deletion to hold
        // re-keyed GamePlayer/ChatMessage/CoinTransaction rows for games that already
        // started — it was never used to sign up or log in. Checked defensively at
        // login so it can never become reachable even if something else slips through.
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        // Age/ToS consent captured at signup. Deliberately does NOT store the raw
        // date of birth (data minimization) — the server computes + validates age
        // at signup time and discards it; birth year only is logged (not persisted)
        // for audit purposes. See AuthController.SignUp.
        public bool AgeVerified { get; set; } = false;
        public DateTimeOffset? AgeVerifiedAt { get; set; }
        public DateTimeOffset? AcceptedTermsAt { get; set; }
        public string? TermsVersion { get; set; }
        // CAN-SPAM opt-out for end-of-game recap emails (the only non-transactional
        // email type left — see GameNotificationService). UnsubscribeToken is
        // lazily generated on first recap send and never expires, since the
        // opt-out link embedded in emails must keep working for 30+ days.
        public bool RecapEmailsOptedOut { get; set; } = false;
        public Guid? UnsubscribeToken { get; set; }
        // CCPA/CPRA "do not sell or share" preference. No ad/analytics vendor exists
        // yet (see architecture.md's privacy-consent gate), so this has no effect
        // today — it's a durable record of the user's choice so it's already honored
        // the moment a vendor is wired up, without needing to re-prompt anyone.
        public bool DoNotSellOrShare { get; set; } = false;
        public DateTimeOffset? DoNotSellOrShareSetAt { get; set; }
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    }
}