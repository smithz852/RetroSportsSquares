using Microsoft.VisualBasic;
using RSS.DTOs;
using RSS_DB;
using RSS_Services.Helpers;
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
        private readonly NbaDataPullHelper _nbaDataPullHelper;
        private readonly FootballMapperHelper _footballMapperHelper;

        public SportsGameServices(AppDbContext appDbContext, HttpClient httpClient, NbaDataPullHelper nbaDataPullHelper, FootballMapperHelper footballMapperHelper)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
            _nbaDataPullHelper = nbaDataPullHelper;
            _footballMapperHelper = footballMapperHelper;
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

        public async Task<List<SportsGamesAvailableDTO>> GetGamesAvailableToday(string sportType, string gameUrl)
        {
            var gamesList = new List<SportsGamesAvailableDTO>();
            try
            {

                var response = await _httpClient.GetAsync(gameUrl);
                //var response = await _httpClient.GetAsync($"https://v1.{sportType}.api-sports.io/games?league=1&season=2022&team=1&timezone=America/Los_Angeles");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var results = document.RootElement.GetProperty("results").GetInt32();

                if (results == 0)
                {
                    return gamesList;
                }

                var responseArray = document.RootElement.GetProperty("response");

                if (sportType == "basketball")
                {
                    _nbaDataPullHelper.GetNbaGameData(responseArray, gamesList, sportType);
                    return gamesList;
                }

                _footballMapperHelper.MapFootballData(responseArray, gamesList, sportType);
                return gamesList; 
            }
            catch (HttpRequestException)
            {
                return gamesList; //need to change later for actual error handling
            }
        }
    }
}
