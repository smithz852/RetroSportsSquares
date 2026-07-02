namespace RSS_Services.DTOs
{
    public class AdminPlayerStatsDTO
    {
        public string UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? GamerTag { get; set; }
        public int GamesPlayed { get; set; }
        public int TotalSquaresClaimed { get; set; }
        public int PeriodsWon { get; set; }
        public decimal TotalWagered { get; set; }
        public decimal WagersWon { get; set; }
        public double WinRate { get; set; }
    }
}
