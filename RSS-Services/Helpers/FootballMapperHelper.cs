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

        public SportScoreUpdateDTO MapFootballScoreData(JsonElement gameElement, string sportType)
        {
            var status = gameElement.GetProperty("game").GetProperty("status").GetProperty("short").GetString()
                ?? gameElement.GetProperty("game").GetProperty("status").GetProperty("long").GetString();

            return new SportScoreUpdateDTO
            {
                ApiGameId = gameElement.GetProperty("game").GetProperty("id").GetInt32(),
                Status = status,
                CurrentHomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("total").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("total").GetInt32(),
                CurrentAwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("total").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("total").GetInt32(),
                Q1HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_1").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_1").GetInt32(),
                Q1AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_1").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_1").GetInt32(),
                Q2HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_2").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_2").GetInt32(),
                Q2AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_2").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_2").GetInt32(),
                Q3HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_3").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_3").GetInt32(),
                Q3AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_3").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_3").GetInt32(),
                Q4HomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_4").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("quarter_4").GetInt32(),
                Q4AwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_4").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("quarter_4").GetInt32(),
                OTHomeScore = gameElement.GetProperty("scores").GetProperty("home").GetProperty("overtime").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("home").GetProperty("overtime").GetInt32(),
                OTAwayScore = gameElement.GetProperty("scores").GetProperty("away").GetProperty("overtime").ValueKind == JsonValueKind.Null ? 0 : gameElement.GetProperty("scores").GetProperty("away").GetProperty("overtime").GetInt32(),
            };
        }
    }
}
