namespace RSS_Services.DTOs
{
    public class AdminCurrentGameDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public string League { get; set; }
        public string Matchup { get; set; }
        public string? HostDisplayName { get; set; }
        public int PlayersJoined { get; set; }
        public int MaxPlayers { get; set; }
        public decimal PricePerSquare { get; set; }
        public int SquaresClaimed { get; set; }
        public bool IsOpen { get; set; }
        public bool SelectionPhaseActive { get; set; }
        public bool IsPublic { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
