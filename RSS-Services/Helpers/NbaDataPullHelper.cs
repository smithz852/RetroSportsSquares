using RSS.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services.Helpers
{
    public class NbaDataPullHelper
    {
        private readonly DataSortHelpers _dataSortHelpers;

        public NbaDataPullHelper(DataSortHelpers dataSortHelpers)
        {
            _dataSortHelpers = dataSortHelpers;
        }
        public void GetNbaGameData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType)
        {
  
            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var leagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32();

                if (leagueId == 12)
                {
                    var gameStartString = gameElement.GetProperty("date").GetString();
                    var gameStartDate = DateTime.Parse(gameStartString);
                    var mergeIntoGameName = _dataSortHelpers.MakeGameName(gameElement);

                    var gameDto = new SportsGamesAvailableDTO
                    {
                        ApiGameId = gameElement.GetProperty("id").GetInt32(),
                        InUse = false,
                        GameStartTime = gameElement.GetProperty("time").GetString(),
                        GameStartDate = gameStartDate,
                        GameName = mergeIntoGameName,
                        Status = gameElement.GetProperty("status").GetProperty("short").GetString(),
                        SportType = sportType,
                        League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                        LeagueId = leagueId
                    };
                    gamesList.Add(gameDto);
                }
                 
            }
        }
    }
}
