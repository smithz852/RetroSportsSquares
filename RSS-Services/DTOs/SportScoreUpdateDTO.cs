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
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
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
        public string? WinnerQ1 { get; set; }
        public string? WinnerQ2 { get; set; }
        public string? WinnerQ3 { get; set; }
        public string? WinnerQ4 { get; set; }
    }
}
