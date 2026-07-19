namespace RSS.DTOs
{
    public class CreateGameDTO
    {
        public string Name { get; set; }
        public string GameType { get; set; }
        public bool IsOpen { get; set; }
        public int PlayerCount { get; set; }
        public decimal PricePerSquare { get; set; }
        public int SquareSelectionLimit { get; set; }
        public bool IsTurnBased { get; set; }
        public int TurnTimeoutSeconds { get; set; } = 60;
        public bool IsPublic { get; set; } = true;
        public string DailySportsGameId { get; set; }
        // Null/omitted means Default (pre-picker clients)
        public string? PayoutMode { get; set; }
    }
}