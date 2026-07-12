using System;

namespace RSS_Services.DTOs
{
    // Returned by SquareServices.SaveQuarterlyWinner when a winner was newly persisted.
    // Null result means the period was already resolved (idempotency guard) — no notifications should fire.
    public class QuarterlyWinnerSaveResult
    {
        public Guid SquareGameId { get; set; }
        public int Period { get; set; }
        public string WinnerApplicationUserId { get; set; } = string.Empty;
    }
}
