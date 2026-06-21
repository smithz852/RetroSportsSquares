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
        private readonly SoccerMapperHelper _soccerMapperHelper;
        private readonly GeneralServices _generalServices;
        private readonly TimeHelpers _timeHelpers;
        private readonly AvailableGamesServices _availableGamesServices;

        public SportsGameServices(AppDbContext appDbContext, HttpClient httpClient, BasketballMapperHelper nbaDataPullHelper, FootballMapperHelper footballMapperHelper, SoccerMapperHelper soccerMapperHelper, GeneralServices generalServices, TimeHelpers timeHelpers, AvailableGamesServices availableGamesServices)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
            _nbaDataPullHelper = nbaDataPullHelper;
            _footballMapperHelper = footballMapperHelper;
            _soccerMapperHelper = soccerMapperHelper;
            _generalServices = generalServices;
            _timeHelpers = timeHelpers;
            _availableGamesServices = availableGamesServices;
        }

        public bool AreGamesInDbForToday(string sportType, int leagueId)
        {
            var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
            return _appDbContext.DailySportsGames
                    .AsEnumerable()
                    .Any(g => TimeZoneInfo.ConvertTime(g.GameStartTime, pacific).Date == todayPst
                        && g.SportType == sportType
                        && g.LeagueId == leagueId);
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

                if (sportType == "soccer")
                {
                    _soccerMapperHelper.GetSoccerGameData(responseArray, gamesList, sportType);
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

        public async Task SaveSportsData(List<SportsGamesAvailableDTO> availableGames)
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
                    SportType = game.SportType,
                    League = game.League,
                    LeagueId = game.LeagueId,
                    Status = game.Status,
                };

                await _generalServices.SaveData(dailySportsGames);
            }
        }

        public List<DailySportsGames> GetAvailableSportsGameOptions(string gameType, int leagueId)
        {
            var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();

            if (gameType == "football")
            {
                gameType = "american-football";
            }

            var availbleGameOptions = _appDbContext.DailySportsGames
                .AsEnumerable()
                .Where(g => TimeZoneInfo.ConvertTime(g.GameStartTime, pacific).Date == todayPst && g.LeagueId == leagueId && g.SportType == gameType)
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

        public void SetGameNotInUse(Guid dailySportsGameId)
        {
            var game = GetDailySportGameById(dailySportsGameId);
            if (game != null)
            {
                game.InUse = false;
                _appDbContext.SaveChanges();
            }
        }

        public async Task UpdateSportsDataAsync(List<SportScoreUpdateDTO> newSportsData)
        {

            foreach (var data in newSportsData)
            {
                var sportsGame = await _appDbContext.DailySportsGames
                    .FirstOrDefaultAsync(g => g.ApiGameId == data.ApiGameId);

                if (sportsGame == null) continue;

                var status = data.Status;

                sportsGame.InUse = !(status == "FT" || status == "AOT" || status == null || status == "Final/OT" || status == "Postponed");
                sportsGame.Status = status;
                sportsGame.CurrentHomeScore = data.CurrentHomeScore;
                sportsGame.CurrentAwayScore = data.CurrentAwayScore;
                sportsGame.HomePeriodScores = data.HomePeriodScores;
                sportsGame.AwayPeriodScores = data.AwayPeriodScores;
            }

            await _appDbContext.SaveChangesAsync();
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

        public async Task<List<SportScoreUpdateDTO>> GetNbaGameData(string gameUrl, string sportType)
        {
            try
            {
                var gamesList = new List<SportScoreUpdateDTO>();
                var response = await _httpClient.GetAsync(gameUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var responseArray = document.RootElement.GetProperty("response");

                foreach (var sportsGame in responseArray.EnumerateArray())
                {
                    var leagueName = sportsGame.GetProperty("league").GetProperty("name").GetString();
                    if (leagueName != "NBA") continue;

                    var game = _nbaDataPullHelper.MapBasketballScoreData(sportsGame, sportType);
                    gamesList.Add(game);
                }
                return gamesList;
            }


            catch (HttpRequestException ex)
            {
                var game = new List<SportScoreUpdateDTO>();

                return game; //need to change later for actual error handling
            }
        }

        public async Task<List<SportScoreUpdateDTO>> GetNflGameData(string gameUrl, string sportType)
        {
            try
            {
                var gamesList = new List<SportScoreUpdateDTO>();
                var response = await _httpClient.GetAsync(gameUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var responseArray = document.RootElement.GetProperty("response");

                foreach (var sportsGame in responseArray.EnumerateArray())
                {
                    var leagueName = sportsGame.GetProperty("league").GetProperty("name").GetString();
                    if (leagueName != "NFL") continue;

                    var game = _footballMapperHelper.MapFootballScoreData(sportsGame, sportType);
                    gamesList.Add(game);
                }
                return gamesList;
            }

            catch (HttpRequestException ex)
            {
                var game = new List<SportScoreUpdateDTO>();

                return game; //need to change later for actual error handling
            }
        }

        public async Task<List<SportScoreUpdateDTO>> GetSoccerGameData(string gameUrl, string sportType)
        {
            try
            {
                var gamesList = new List<SportScoreUpdateDTO>();
                var response = await _httpClient.GetAsync(gameUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var responseArray = document.RootElement.GetProperty("response");

                foreach (var sportsGame in responseArray.EnumerateArray())
                {
                    var game = _soccerMapperHelper.MapSoccerScoreData(sportsGame, sportType);
                    gamesList.Add(game);
                }
                return gamesList;
            }
            catch (HttpRequestException)
            {
                return new List<SportScoreUpdateDTO>();
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

        public async Task<List<DailySportsGames>> GetAllGamesBySporttType(string sportType)
        {
            var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
            var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(todayPst, pacific);
            var todayEndUtc = todayStartUtc.AddDays(1);

            return await _appDbContext.DailySportsGames
                .Where(g => g.SportType == sportType
                    && g.GameStartTime >= todayStartUtc
                    && g.GameStartTime < todayEndUtc)
                .ToListAsync();
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
