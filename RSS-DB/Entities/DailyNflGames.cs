using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Entities
{
    public class DailyNflGames
    {
        public Guid Id { get; set; }
        public string ApiGameId { get; set; }
        public bool InUse { get; set; }
        public string GameName { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; }
    }
}
