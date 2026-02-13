namespace RSS.DTOs
{
    public class SportsGamesAvailableDTO
    {
        public int ApiGameId { get; set; }
        public bool InUse { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public DateTimeOffset GameStartTime { get; set; }
        public DateTimeOffset GameStartDate { get; set; }
        public string Status { get; set; }
        public string SportType { get; set; }
        public string League { get; set; }
        public int LeagueId { get; set; }
    }
}
