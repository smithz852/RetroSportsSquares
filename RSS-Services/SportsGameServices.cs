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
using RSS_Services.DTOs;

namespace RSS_Services
{
    public class SportsGameServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;
        private readonly BasketballMapperHelper _nbaDataPullHelper;
        private readonly FootballMapperHelper _footballMapperHelper;
        private readonly GeneralServices _generalServices;
        private readonly TimeHelpers _timeHelpers;
        private readonly AvailableGamesServices _availableGamesServices;

        public SportsGameServices(AppDbContext appDbContext, HttpClient httpClient, BasketballMapperHelper nbaDataPullHelper, FootballMapperHelper footballMapperHelper, GeneralServices generalServices, TimeHelpers timeHelpers, AvailableGamesServices availableGamesServices)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
            _nbaDataPullHelper = nbaDataPullHelper;
            _footballMapperHelper = footballMapperHelper;
            _generalServices = generalServices;
            _timeHelpers = timeHelpers;
            _availableGamesServices = availableGamesServices;

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

                DailySportsGames dailySportsGames = new DailySportsGames()
                {
                    ApiGameId = game.ApiGameId,
                    InUse = game.InUse,
                    HomeTeam = game.HomeTeam,
                    AwayTeam = game.AwayTeam,
                    GameStartTime = game.GameStartTime,
                    GameStartDate = game.GameStartDate,
                    SportType = game.SportType,
                    League = game.League,
                    LeagueId = game.LeagueId,
                    Status = game.Status,
                };

                _generalServices.SaveData(dailySportsGames);
            }
        }

        public List<DailySportsGames> GetAvailableSportsGameOptions(string gameType, int leagueId)
        {
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
            if (gameType == "football")
            {
                gameType = "american-football";
            }
                

            //for testing delete after
            //var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            //var customDate = new DateTime(2026, 1, 31);
            //var todayTest = TimeZoneInfo.ConvertTimeFromUtc(customDate, pstZone).Date;
            //var dateString = todayTest.ToString("yyyy-MM-dd");
            //var todayPst = DateTime.Parse(dateString);

            var availbleGameOptions = _appDbContext.DailySportsGames
                .Where(g => g.GameStartDate.Date == todayPst && g.LeagueId == leagueId && g.SportType == gameType)
                .ToList(); //add check for status != FT or AOT later
            return availbleGameOptions;
        }

        public void SetGameInUse(string dailySportsGameId)
        {
            var game = GetDailySportGameById(dailySportsGameId);
            if (game != null)
            {
                game.InUse = true;
            }
            _appDbContext.SaveChanges();
        }

        public void UpdateSportsData(SportScoreUpdateDTO newSportsData, Guid Id)
        {
            var sportsGame = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == Id);

            if (sportsGame != null)
            {
                var inUse = sportsGame.InUse;
                var status = newSportsData.Status;

                if (status == "FT" || status == "AOT" || status == null || status == "Final/OT" || status == "Postponed")
                {
                    inUse = false;

                }

                if (sportsGame != null)
                {
                    sportsGame.InUse = inUse;
                    sportsGame.Status = status;
                    sportsGame.CurrentHomeScore = newSportsData.CurrentHomeScore;
                    sportsGame.CurrentAwayScore = newSportsData.CurrentAwayScore;
                    sportsGame.Q1HomeScore = newSportsData.Q1HomeScore;
                    sportsGame.Q1AwayScore = newSportsData.Q1AwayScore;
                    sportsGame.Q2HomeScore = newSportsData.Q2HomeScore;
                    sportsGame.Q2AwayScore = newSportsData.Q2AwayScore;
                    sportsGame.Q3HomeScore = newSportsData.Q3HomeScore;
                    sportsGame.Q3AwayScore = newSportsData.Q3AwayScore;
                    sportsGame.Q4HomeScore = newSportsData.Q4HomeScore;
                    sportsGame.Q4AwayScore = newSportsData.Q4AwayScore;
                    sportsGame.OTHomeScore = newSportsData.OTHomeScore;
                    sportsGame.OTAwayScore = newSportsData.OTAwayScore;
                }
            }

           _appDbContext.SaveChanges();
        }

        public async Task<SportScoreUpdateDTO> GetSportsGameDataByGameId(string gameUrl, string sportType)
        {

            try
            {
                var response = await _httpClient.GetAsync(gameUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var responseArray = document.RootElement.GetProperty("response");

                if (sportType == "basketball")
                {
                    var basketballGame = _nbaDataPullHelper.MapBasketballScoreData(responseArray, sportType);
                    return basketballGame;
                }

                var game =_footballMapperHelper.MapFootballScoreData(responseArray, sportType);
                return game;
            }
            catch (HttpRequestException)
            {
                var game = new SportScoreUpdateDTO();
                return game; //need to change later for actual error handling
            }
        }

        public int GetGameApiIdFromId(Guid id)
        {
            var game = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == id);
            return game.ApiGameId;
        }

        public DailySportsGames GetDailySportGameById(string dailySportsGameId)
        {
            var dailySportsGameGuid = Guid.Parse(dailySportsGameId);
            var game = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameGuid);
            return game;
        }

        //Guid overload 
        public DailySportsGames GetDailySportGameById(Guid dailySportsGameId)
        {
            var game = _appDbContext.DailySportsGames.FirstOrDefault(g => g.Id == dailySportsGameId);
            return game;
        }

        public List<SportsGamesInUseDTO> GetAllGamesInUse(string sportType)
        {
            var allGameInUse = _appDbContext.DailySportsGames
                .Where(g => g.InUse == true && g.SportType == sportType)
                .Select(g => new SportsGamesInUseDTO
                {
                    Id = g.Id,
                })
                .ToList();
            return allGameInUse;
        }

        public bool HasGameStarted(Guid gameId)
        {
            var game = GetDailySportGameById(gameId);
            var startTime = game.GameStartTime;

            if (DateTimeOffset.UtcNow >= startTime)
            {
                return true;
            }
            return false;
        }

    }
}
