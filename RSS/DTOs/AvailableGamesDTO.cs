namespace RSS.DTOs
{
    public class AvailableGamesDTO
    {
        public string GameName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal PlayerCount { get; set; }
    }
}
