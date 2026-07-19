using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS.DTOs
{
    public class SportScoreUpdateDTO
    {
        public int ApiGameId { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public string Status { get; set; }
        public string SportType { get; set; }
        public int CurrentHomeScore { get; set; }
        public int CurrentAwayScore { get; set; }
        public List<int> HomePeriodScores { get; set; } = new();
        public List<int> AwayPeriodScores { get; set; } = new();
        // Populated only for the score endpoint response (display names keyed by period number)
        public Dictionary<int, string?> PeriodWinners { get; set; } = new();
        public decimal PayoutPerPeriod { get; set; }
        // Score-endpoint only: lets the board render mode mechanics (e.g. Push pot)
        public string? PayoutMode { get; set; }
        public int PeriodCount { get; set; }
    }
}
