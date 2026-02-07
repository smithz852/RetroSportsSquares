namespace RSS.DTOs
{
    public class AvailableGamesDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOpen { get; set; }
        public decimal PlayerCount { get; set; }
        public decimal PricePerSquare { get; set; }
        public int SportGameId { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
    }
}
