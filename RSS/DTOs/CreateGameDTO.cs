namespace RSS.DTOs
{
    public class CreateGameDTO
    {
        public string Name { get; set; }
        public string GameType { get; set; }
        public bool IsOpen { get; set; }
        public int PlayerCount { get; set; }
        public int PricePerSquare { get; set; }
        public string DailySportsGameId { get; set; }
    }
}