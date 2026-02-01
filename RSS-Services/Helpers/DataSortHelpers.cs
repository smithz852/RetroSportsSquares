using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services.Helpers
{
    public class DataSortHelpers
    {
        public string MakeGameName(JsonElement gameElement)
        {
            var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
            var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
            var mergeIntoGameName = awayTeamName + " VS " + homeTeamName;
            return mergeIntoGameName;
        }
    }
}
