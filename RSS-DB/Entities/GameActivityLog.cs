using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSS_DB.Entities
{
    public static class GameActivityTypes
    {
        public const string Start = "Start";
        public const string Join = "Join";
    }

    // Daily per-user quota ledger for game starts (2/day) and joins (5/day) — Admins
    // are exempt, see GameQuotaService. Join rows are removed on LeaveGame or when the
    // host deletes an unstarted game, so backing out of a game you haven't committed
    // to yet doesn't cost a slot; Start rows are permanent — a game can't be deleted
    // once started.
    public class GameActivityLog
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        // Soft reference only (no FK) — mirrors CoinTransaction.SquareGameId
        public Guid GameId { get; set; }
        [Required]
        [MaxLength(10)]
        public string Type { get; set; }
        [Column(TypeName = "date")]
        public DateTime ActivityDatePst { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public GameActivityLog()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
