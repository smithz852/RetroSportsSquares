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
        public string GameName { get; set; }
        [Required]
        public string GameStartTime { get; set; }
        [Required]
        public DateTime GameStartDate { get; set; }
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
        public string CurrentQuarter { get; set; } = string.Empty;
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
        public virtual List<AvailableGames> AvailableGames { get; set; }

        public DailySportsGames()
        {
            Id = Guid.NewGuid();
        }
    }
}
