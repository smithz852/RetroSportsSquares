namespace RSS_Services.DTOs
{
    public class CurrentGameSummaryDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameType { get; set; }
        public decimal PricePerSquare { get; set; }
        public int SquaresClaimed { get; set; }
        public bool IsHost { get; set; }
        public bool IsOpen { get; set; }
        public bool SelectionPhaseActive { get; set; }
    }
}
