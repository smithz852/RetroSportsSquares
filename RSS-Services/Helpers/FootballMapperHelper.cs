using RSS.DTOs;
using RSS_Services.DTOs;
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

        public FootballMapperHelper()
        {

        }
        public void MapFootballData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType)
        {

            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                var status = gameElement.GetProperty("game").GetProperty("status").GetProperty("short").GetString();
                var gameStartTimeUnix = gameElement.GetProperty("game").GetProperty("date").GetProperty("timestamp").GetInt64();
                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(gameStartTimeUnix);

                if (status == null)
                {
                    var statusLong = gameElement.GetProperty("game").GetProperty("status").GetProperty("long").GetString();
                    status = statusLong;
                }

                var gameDto = new SportsGamesAvailableDTO
                {
                    ApiGameId = gameElement.GetProperty("game").GetProperty("id").GetInt32(),
                    InUse = false,
                    GameStartTime = gameStartTime, 
                    HomeTeam = homeTeamName,
                    AwayTeam = awayTeamName,
                    Status = status,
                    SportType = sportType,
                    League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                    LeagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32()
                };
                gamesList.Add(gameDto);
            }
        }

        public SportScoreUpdateDTO MapFootballScoreData(JsonElement responseArray, string sportType)
        {
            var gameDto = new SportScoreUpdateDTO();

            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var status = gameElement.GetProperty("game").GetProperty("status").GetProperty("short").GetString();

                if (status == null)
                {
                    var statusLong = gameElement.GetProperty("game").GetProperty("status").GetProperty("long").GetString();
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
                gameDto.OTHomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("overtime").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("overtime").GetInt32();
                gameDto.OTAwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("overtime").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("overtime").GetInt32();

            }
            return gameDto;
        }
    }
}
