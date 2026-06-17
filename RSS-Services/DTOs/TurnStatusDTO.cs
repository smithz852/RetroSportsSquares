namespace RSS_Services.DTOs
{
    public class TurnStatusDTO
    {
        public bool SelectionPhaseActive { get; set; }
        public string? CurrentTurnUserId { get; set; }
        public DateTimeOffset? TurnStartedAt { get; set; }
        public int TurnTimeoutSeconds { get; set; }
        public List<TurnPlayerDTO> Players { get; set; } = new();
    }

    public class TurnPlayerDTO
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public int TurnOrder { get; set; }
        public bool HasHadTurn { get; set; }
    }
}
