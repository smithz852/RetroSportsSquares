using RSS.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services.Helpers
{
    public class FootballMapperHelper
    {
        public void MapFootballData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType)
        {

            foreach (var gameElement in responseArray.EnumerateArray())
            {
                //move to helper after
                var gameStartString = gameElement.GetProperty("game").GetProperty("date").GetProperty("date").GetString();
                var gameStartDate = DateTime.Parse(gameStartString);

                //move to helper after
                var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                var mergeIntoGameName = awayTeamName + " VS " + homeTeamName;

                var status = gameElement.GetProperty("game").GetProperty("status").GetProperty("short").GetString();

                if (status == null)
                {
                    status = "OT";
                }

                var gameDto = new SportsGamesAvailableDTO
                {
                    ApiGameId = gameElement.GetProperty("game").GetProperty("id").GetInt32(),
                    InUse = false,
                    GameStartTime = gameElement.GetProperty("game").GetProperty("date").GetProperty("time").GetString(),
                    GameStartDate = gameStartDate,
                    GameName = mergeIntoGameName,
                    Status = status,
                    SportType = sportType,
                    League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                    LeagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32()
                };
                gamesList.Add(gameDto);
            }
        }
    }
}
