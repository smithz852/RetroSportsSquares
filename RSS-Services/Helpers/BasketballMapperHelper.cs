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
        public void GetBasketballGameData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType, int[] leagueIds)
        {
            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var leagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32();
                if (!leagueIds.Contains(leagueId)) continue;

                var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                var gameStartTimeUnix = gameElement.GetProperty("timestamp").GetInt64();
                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(gameStartTimeUnix);
                var status = gameElement.GetProperty("status").GetProperty("short").GetString()
                    ?? gameElement.GetProperty("status").GetProperty("long").GetString();

                var gameDto = new SportsGamesAvailableDTO
                {
                    ApiGameId = gameElement.GetProperty("id").GetInt32(),
                    InUse = false,
                    GameStartTime = gameStartTime,
                    HomeTeam = homeTeamName,
                    AwayTeam = awayTeamName,
                    Status = status,
                    SportType = sportType,
                    League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                    LeagueId = leagueId
                };
                gamesList.Add(gameDto);
            }
        }

        public SportScoreUpdateDTO MapBasketballScoreData(JsonElement gameElement, string sportType)
        {
            var status = gameElement.GetProperty("status").GetProperty("short").GetString()
                ?? gameElement.GetProperty("status").GetProperty("long").GetString();

            int GetScore(string side, string key)
            {
                var el = gameElement.GetProperty("scores").GetProperty(side).GetProperty(key);
                return el.ValueKind == JsonValueKind.Null ? 0 : el.GetInt32();
            }

            return new SportScoreUpdateDTO
            {
                ApiGameId = gameElement.GetProperty("id").GetInt32(),
                Status = status,
                SportType = sportType,
                CurrentHomeScore = GetScore("home", "total"),
                CurrentAwayScore = GetScore("away", "total"),
                HomePeriodScores = new List<int>
                {
                    GetScore("home", "quarter_1"),
                    GetScore("home", "quarter_2"),
                    GetScore("home", "quarter_3"),
                    GetScore("home", "quarter_4"),
                },
                AwayPeriodScores = new List<int>
                {
                    GetScore("away", "quarter_1"),
                    GetScore("away", "quarter_2"),
                    GetScore("away", "quarter_3"),
                    GetScore("away", "quarter_4"),
                },
            };
        }
    }
}
