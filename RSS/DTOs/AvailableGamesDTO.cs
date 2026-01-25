namespace RSS.DTOs
{
    public class AvailableGamesDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal PlayerCount { get; set; }
    }
}
