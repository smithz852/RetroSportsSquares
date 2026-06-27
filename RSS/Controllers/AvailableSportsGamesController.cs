using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSS.DTOs;
using RSS.Helpers;
using RSS_Services;

namespace RSS.Controllers
{
[Route("[controller]")]
[ApiController]
    public class AvailableSportsGamesController : ControllerBase
    {
        private SportsGameServices _sportsGameServices;
        private MapperHelpers _mapperHelpers;

        public AvailableSportsGamesController(SportsGameServices sportsGameServices, MapperHelpers mapperHelpers)
        {
            _sportsGameServices = sportsGameServices;
            _mapperHelpers = mapperHelpers;
        }

        [HttpGet("GetAvailable{gameType}League{leagueId}GameOptions")]
        public IActionResult GetAvailableNflGameOptions(string gameType, string leagueId)
        {
            int leagueIdStringToInt = int.Parse(leagueId);
           var availableGameOptions = _sportsGameServices.GetAvailableSportsGameOptions(gameType, leagueIdStringToInt);
            var availableSportsGamesOptionsDTO = new List<AvailableSportsGamesOptionsDTO>();

            if (availableGameOptions == null)
            {
                return Ok(new List<AvailableSportsGamesOptionsDTO>());
            }

            foreach (var gameOption in availableGameOptions)
            {
               var availableSportsGameOptions = _mapperHelpers.AvailableSportsGamesOptionsMapper(gameOption);
               availableSportsGamesOptionsDTO.Add(availableSportsGameOptions);
            }
            return Ok(availableSportsGamesOptionsDTO);
        }

        [HttpGet("GetAvailableSportsAndLeagues")]
        public IActionResult GetAvailableSportsAndLeagues()
        {
            var results = _sportsGameServices.GetAvailableSportLeaguesToday();

            var dto = results.Select(r => new AvailableSportLeagueDTO
            {
                SportType = r.SportType == "american-football" ? "football" : r.SportType,
                League = r.League,
                LeagueId = r.LeagueId,
            }).ToList();

            return Ok(dto);
        }

    }
}
