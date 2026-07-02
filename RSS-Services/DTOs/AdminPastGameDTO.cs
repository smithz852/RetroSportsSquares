namespace RSS_Services.DTOs
{
    public class AdminPastGameDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public string League { get; set; }
        public string Matchup { get; set; }
        public string? HostDisplayName { get; set; }
        public int PlayersJoined { get; set; }
        public decimal PricePerSquare { get; set; }
        public decimal TotalPot { get; set; }
        public List<AdminPeriodWinnerDTO> PeriodWinners { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class AdminPeriodWinnerDTO
    {
        public int Period { get; set; }
        public string? WinnerDisplayName { get; set; }
    }

    public class AdminPaginatedPastGamesDTO
    {
        public List<AdminPastGameDTO> Games { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
