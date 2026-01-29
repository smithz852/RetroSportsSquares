using Microsoft.VisualBasic;
using RSS.DTOs;
using RSS_DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class SportsGameServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;

        public SportsGameServices(AppDbContext appDbContext, HttpClient httpClient)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
        }

        public bool AreGamesInDbForToday(string sportType, int leagueId)
        {
            //get from helper once made below
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
            //make query specific to sport
            return _appDbContext.DailySportsGames
                .Any(g => g.GameStartDate.Date == todayPst && g.SportType == sportType && g.LeagueId == leagueId);
        }

        public async Task<List<SportsGamesAvailableDTO>> GetGamesAvailableToday(string sportType, int leagueId)
        {
            var gamesList = new List<SportsGamesAvailableDTO>();
            try
            {
                //move to helper after
                var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
                var dateString = todayPst.ToString("yyyy-MM-dd");

                var response = await _httpClient.GetAsync($"https://v1.{sportType}.api-sports.io/games?league={leagueId}&date={dateString}&timezone=America/Los_Angeles");
                //var response = await _httpClient.GetAsync($"https://v1.{sportType}.api-sports.io/games?league={leagueId}&season=2022&team=1&timezone=America/Los_Angeles");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                
                using var document = JsonDocument.Parse(json);
                var results = document.RootElement.GetProperty("results").GetInt32();

                if (results == 0)
                {
                    return gamesList;
                }

                //possibly move to mapper helper
                var responseArray = document.RootElement.GetProperty("response");
                foreach (var gameElement in responseArray.EnumerateArray())
                {
                    //move to helper after
                    var gameStartString = gameElement.GetProperty("game").GetProperty("date").GetProperty("date").GetString();
                    var gameStartDate = DateTime.Parse(gameStartString);

                    //move to helper after
                    var homeTeamName = gameElement.GetProperty("teams").GetProperty("home").GetProperty("name").GetString();
                    var awayTeamName = gameElement.GetProperty("teams").GetProperty("away").GetProperty("name").GetString();
                    var mergeIntoGameName = awayTeamName + " VS " + homeTeamName;

                    var gameDto = new SportsGamesAvailableDTO
                    {
                        ApiGameId = gameElement.GetProperty("game").GetProperty("id").GetInt32(),
                        InUse = false,
                        GameStartTime = gameElement.GetProperty("game").GetProperty("date").GetProperty("time").GetString(),
                        GameStartDate = gameStartDate,
                        GameName = mergeIntoGameName,
                        Status = gameElement.GetProperty("game").GetProperty("status").GetProperty("short").GetString(),
                        SportType = sportType,
                        League = gameElement.GetProperty("league").GetProperty("name").GetString(),
                        LeagueId = gameElement.GetProperty("league").GetProperty("id").GetInt32()
                    };
                    gamesList.Add(gameDto);
                }

                return gamesList; 
            }
            catch (HttpRequestException)
            {
                return gamesList; //need to change later for actual error handling
            }
        }
    }
}
