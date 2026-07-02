namespace RSS_Services.DTOs
{
    public class AdminUserSummaryDTO
    {
        public string Id { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? GamerTag { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int GamesPlayed { get; set; }
        public bool IsAdmin { get; set; }
    }
}
