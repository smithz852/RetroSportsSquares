using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class DailySportsGames
    {
        public Guid Id { get; set; }
        [Required]
        public int ApiGameId { get; set; }
        [Required]
        public bool InUse { get; set; }
        [Required]
        public string HomeTeam { get; set; }
        [Required]
        public string AwayTeam { get; set; }
        [Required]
        public DateTimeOffset GameStartTime { get; set; } //in utc by timestamp unix
        [Required]
        public string SportType { get; set; }
        [Required]
        public string League {  get; set; }
        [Required]
        public int LeagueId { get; set; }
        [Required]
        public string Status { get; set; }
        public int CurrentHomeScore { get; set; }
        public int CurrentAwayScore { get; set; }
        public List<int> HomePeriodScores { get; set; } = new();
        public List<int> AwayPeriodScores { get; set; } = new();
        public ICollection<SquareGames> AvailableGames { get; set; } = new List<SquareGames>();

        public DailySportsGames()
        {
            Id = Guid.NewGuid();
        }
    }
}
