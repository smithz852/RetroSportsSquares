using RSS.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services.Helpers
{
    public class BasketballMapperHelper
    {

        public BasketballMapperHelper()
        {

        }
        public void GetNbaGameData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType)
        {
  
            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var leagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32();
                var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                var gameStartTimeUnix = gameElement.GetProperty("timestamp").GetInt64();
                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(gameStartTimeUnix);

                if (leagueId == 12)
                {
                    var gameStartString = gameElement.GetProperty("date").GetString();
                    var gameStartDate = DateTime.Parse(gameStartString);

                    var gameDto = new SportsGamesAvailableDTO
                    {
                        ApiGameId = gameElement.GetProperty("id").GetInt32(),
                        InUse = false,
                        GameStartTime = gameStartTime,
                        GameStartDate = gameStartDate,
                        HomeTeam = homeTeamName,
                        AwayTeam = awayTeamName,
                        Status = gameElement.GetProperty("status").GetProperty("short").GetString(),
                        SportType = sportType,
                        League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                        LeagueId = leagueId
                    };
                    gamesList.Add(gameDto);
                }
                 
            }
        }

        public SportScoreUpdateDTO MapBasketballScoreData(JsonElement responseArray, string sportType)
        {
            var gameDto = new SportScoreUpdateDTO();

            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var status = gameElement.GetProperty("status").GetProperty("short").GetString();

                if (status == null)
                {
                    var statusLong = gameElement.GetProperty("status").GetProperty("long").GetString();
                    status = statusLong;
                }

                gameDto.Status = status;
                gameDto.CurrentHomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("total").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("total").GetInt32();
                gameDto.CurrentAwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("total").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("total").GetInt32();
                gameDto.Q1HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_1").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_1").GetInt32();
                gameDto.Q1AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_1").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_1").GetInt32();
                gameDto.Q2HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_2").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_2").GetInt32();
                gameDto.Q2AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_2").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_2").GetInt32();
                gameDto.Q3HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_3").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_3").GetInt32();
                gameDto.Q3AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_3").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_3").GetInt32();
                gameDto.Q4HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_4").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_4").GetInt32();
                gameDto.Q4AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_4").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_4").GetInt32();
                gameDto.OTHomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("over_time").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("over_time").GetInt32();
                gameDto.OTAwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("over_time").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("over_time").GetInt32();

            }
            return gameDto;
        }
    }
}
