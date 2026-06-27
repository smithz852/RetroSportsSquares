namespace RSS_Services.DTOs
{
    public class PastGameSummaryDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public decimal PricePerSquare { get; set; }
        public int SquaresClaimed { get; set; }
        public int PeriodsWon { get; set; }
        public decimal TotalWagered { get; set; }
        public decimal TotalWon { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class PaginatedPastGamesDTO
    {
        public List<PastGameSummaryDTO> Games { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
