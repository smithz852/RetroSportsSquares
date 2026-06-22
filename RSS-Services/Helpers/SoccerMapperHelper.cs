using RSS.DTOs;
using RSS_Services.DTOs;
using System.Collections.Generic;
using System.Text.Json;

namespace RSS_Services.Helpers
{
    public class SoccerMapperHelper
    {
        // TODO: Confirm exact JSON property paths from the soccer API response
        public void MapSoccerGameData(JsonElement responseArray, List<SportsGamesAvailableDTO> gamesList, string sportType, int[] leagueIds)
        {
            foreach (var gameElement in responseArray.EnumerateArray())
            {
                var leagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32();
                if (!leagueIds.Contains(leagueId)) continue;

                var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                var status = gameElement.GetProperty("fixture").GetProperty("status").GetProperty("short").GetString();

                if (status == null)
                {
                    var statusLong = gameElement.GetProperty("fixture").GetProperty("status").GetProperty("long").GetString();
                    status = statusLong;
                }

                var gameStartTimeUnix = gameElement.GetProperty("fixture").GetProperty("timestamp").GetInt64();
                var gameStartTime = DateTimeOffset.FromUnixTimeSeconds(gameStartTimeUnix);
                var leagueName = gameElement.GetProperty("league").GetProperty("name").GetString();
                var apiGameId = gameElement.GetProperty("fixture").GetProperty("id").GetInt32();

                var gameDto = new SportsGamesAvailableDTO
                {
                    ApiGameId = apiGameId,
                    InUse = false,
                    GameStartTime = gameStartTime,
                    HomeTeam = homeTeamName,
                    AwayTeam = awayTeamName,
                    Status = status,
                    SportType = sportType,
                    League = leagueName,
                    LeagueId = leagueId
                };
                gamesList.Add(gameDto);
            }
        }

        public SportScoreUpdateDTO MapSoccerScoreData(JsonElement gameElement, string sportType)
        {
            // TODO: Confirm exact JSON property paths from the soccer API score response shape
            var status = gameElement.GetProperty("fixture").GetProperty("status").GetProperty("short").GetString()
                ?? gameElement.GetProperty("fixture").GetProperty("status").GetProperty("long").GetString();

            int GetGoals(string side, string period)
            {
                // TODO: Adjust nested path if the API returns halftime/fulltime scores differently
                var el = gameElement.GetProperty("score").GetProperty(period).GetProperty(side);
                return el.ValueKind == JsonValueKind.Null ? 0 : el.GetInt32();
            }

            var h1Home = GetGoals("home", "halftime");
            var h1Away = GetGoals("away", "halftime");

            // H2 score = fulltime total minus halftime (period scores, not cumulative)
            var ftHome = gameElement.GetProperty("goals").GetProperty("home").ValueKind == JsonValueKind.Null
                ? 0 : gameElement.GetProperty("goals").GetProperty("home").GetInt32();
            var ftAway = gameElement.GetProperty("goals").GetProperty("away").ValueKind == JsonValueKind.Null
                ? 0 : gameElement.GetProperty("goals").GetProperty("away").GetInt32();

            var h2Home = ftHome - h1Home;
            var h2Away = ftAway - h1Away;

            return new SportScoreUpdateDTO
            {
                ApiGameId = gameElement.GetProperty("fixture").GetProperty("id").GetInt32(),
                Status = status,
                SportType = sportType,
                CurrentHomeScore = ftHome,
                CurrentAwayScore = ftAway,
                HomePeriodScores = new List<int> { h1Home, h2Home },
                AwayPeriodScores = new List<int> { h1Away, h2Away },
            };
        }
    }
}
