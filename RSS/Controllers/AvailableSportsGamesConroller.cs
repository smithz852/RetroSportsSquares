using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSS.DTOs;
using RSS.Helpers;
using RSS_Services;

namespace RSS.Controllers
{
[Route("api/[controller]")]
[ApiController]
    public class AvailableSportsGamesConroller : ControllerBase
    {
        private SportsGameServices _sportsGameServices;
        private MapperHelpers _mapperHelpers;

        public AvailableSportsGamesConroller(SportsGameServices sportsGameServices, MapperHelpers mapperHelpers)
        {
            _sportsGameServices = sportsGameServices;
            _mapperHelpers = mapperHelpers;
        }

        [HttpGet("GetAvailableGameOptions")]
        public IActionResult GetAvailableNflGameOptions()
        {
            int leagueId = 1;
           var availableGameOptions = _sportsGameServices.GetAvailableSportsGameOptions(leagueId);
            var availableSportsGamesOptionsDTO = new List<AvailableSportsGamesOptionsDTO>();

            if (availableGameOptions == null)
            {
                return Ok(new List<AvailableSportsGamesOptionsDTO>());
            }

            foreach (var gameOption in availableGameOptions)
            {
               var availableNflOption = _mapperHelpers.AvailableSportsGamesOptionsMapper(gameOption);
               availableSportsGamesOptionsDTO.Add(availableNflOption);
            }
            return Ok(availableSportsGamesOptionsDTO);
        }
    }
}
