namespace RSS_Services.DTOs
{
    public class AdminSummaryDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveGames { get; set; }
        public int CompletedGames { get; set; }
        public decimal TotalCoinsWagered { get; set; }
    }
}
