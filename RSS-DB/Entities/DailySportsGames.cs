using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class DailySportsGames
    {
        public Guid Id { get; set; }
        public string ApiGameId { get; set; }
        public bool InUse { get; set; }
        public string GameName { get; set; }
        public string GameStartTime { get; set; }
        public DateTime GameStartDate { get; set; }
        public string SportType { get; set; }
        public string League {  get; set; }
        public int LeagueId { get; set; }
        public string Status { get; set; }
    }
}
