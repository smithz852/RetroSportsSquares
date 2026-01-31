using Microsoft.VisualBasic;
using RSS.DTOs;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using Microsoft.EntityFrameworkCore;
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
        private readonly GeneralServices _generalServices;
        private readonly TimeHelpers _timeHelpers;

        public SportsGameServices(AppDbContext appDbContext, HttpClient httpClient, NbaDataPullHelper nbaDataPullHelper, FootballMapperHelper footballMapperHelper, GeneralServices generalServices, TimeHelpers timeHelpers)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
            _nbaDataPullHelper = nbaDataPullHelper;
            _footballMapperHelper = footballMapperHelper;
            _generalServices = generalServices;
            _timeHelpers = timeHelpers;
        }

        public bool AreGamesInDbForToday(string sportType, int leagueId)
        {
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
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

        public void SaveSportsData(List<SportsGamesAvailableDTO> availableGames)
        {
            foreach (var game in availableGames)
            {
                //temp delete after testing is done
                var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var customDate = new DateTime(2026, 1, 31);
                var todayPst = TimeZoneInfo.ConvertTimeFromUtc(customDate, pstZone).Date;
                var dateString = todayPst.ToString("yyyy-MM-dd");
                var testGameData = DateTime.Parse(dateString);


                DailySportsGames dailySportsGames = new DailySportsGames()
                {
                    ApiGameId = game.ApiGameId,
                    InUse = game.InUse,
                    GameName = game.GameName,
                    GameStartTime = game.GameStartTime,
                    GameStartDate = testGameData,
                    SportType = game.SportType,
                    League = game.League,
                    LeagueId = game.LeagueId,
                    Status = game.Status,
                };
               _generalServices.SaveData(dailySportsGames);
            }
        }

        public List<DailySportsGames> GetAvailableSportsGameOptions(int leagueId)
        {
            //var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();

            //for testing delete after
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var customDate = new DateTime(2026, 1, 31);
            var todayTest = TimeZoneInfo.ConvertTimeFromUtc(customDate, pstZone).Date;
            var dateString = todayTest.ToString("yyyy-MM-dd");
            var todayPst = DateTime.Parse(dateString);

            var availbleGameOptions = _appDbContext.DailySportsGames
                .Where(g => g.GameStartDate.Date == todayPst && g.LeagueId == leagueId)
                .ToList(); //add check for status != FT or AOT later
            return availbleGameOptions;
        }

        public void SetGameInUse(string dailySportsGameId)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var game = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid);
            if (game != null)
            {
                game.InUse = true;
            }
            _appDbContext.SaveChanges();
        }

    }
}
