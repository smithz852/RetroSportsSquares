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
        public DateTimeOffset GameStartTime { get; set; }
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
        public int Q1HomeScore { get; set; }
        public int Q1AwayScore { get; set; }
        public int Q2HomeScore { get; set; }
        public int Q2AwayScore { get; set; }
        public int Q3HomeScore { get; set; }
        public int Q3AwayScore { get; set; }
        public int Q4HomeScore { get; set; }
        public int Q4AwayScore { get; set; }
        public int OTHomeScore { get; set; }
        public int OTAwayScore { get; set; }
        public ICollection<SquareGames> AvailableGames { get; set; } = new List<SquareGames>();

        public DailySportsGames()
        {
            Id = Guid.NewGuid();
        }
    }
}
