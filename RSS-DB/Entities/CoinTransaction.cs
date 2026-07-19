using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSS_DB.Entities
{
    public static class CoinTransactionTypes
    {
        public const string DailyGrant = "DailyGrant";
        public const string Wager = "Wager";
        public const string Refund = "Refund";
        // Settlement credits (game completion)
        public const string PeriodWin = "PeriodWin";
        public const string Redistribution = "Redistribution";
    }

    public class CoinTransaction
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        // Soft reference only (no FK) so ledger history survives game deletion.
        public Guid? SquareGameId { get; set; }
        // Signed: wagers negative, grants/refunds/payouts positive.
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(30)]
        public string Type { get; set; }
        // Set only on DailyGrant rows; the unique (user, date) index makes the
        // daily grant idempotent under concurrent requests. Null everywhere else
        // (MySQL unique indexes permit repeated NULLs).
        [Column(TypeName = "date")]
        public DateTime? GrantDate { get; set; }
        // Which game period a PeriodWin credit came from; null for other types.
        public int? Period { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public CoinTransaction()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
