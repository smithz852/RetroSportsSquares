using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RSS_Services;
using RSS.Helpers;
using RSS.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AvailableGamesController : ControllerBase
    {
        private readonly AvailableGamesServices _availableGamesServices;
        private readonly MapperHelpers _mapperHelpers;
        private readonly GeneralServices _generalServices;
        private readonly SportsGameServices _sportsGameServices;

        public AvailableGamesController(AvailableGamesServices availableGamesServices, MapperHelpers mapperHelpers, GeneralServices generalServices, SportsGameServices sportsGameServices)
        {
            _availableGamesServices = availableGamesServices;
            _mapperHelpers = mapperHelpers;
            _generalServices = generalServices;
            _sportsGameServices = sportsGameServices;
        }

        [HttpGet("GetAvailableGames")]
        public IActionResult GetAvailableGames()
        {
            var availableGames = _availableGamesServices.GetAllAvailableGames();
            var gamesDtoList = new List<AvailableGamesDTO>();

            if (availableGames == null || !availableGames.Any())
            {
                return Ok(new List<AvailableGamesDTO>());
            }

            foreach (var availableGame in availableGames)
            {
                var gameDto = _mapperHelpers.AvailableGamesMapper(availableGame);
                gamesDtoList.Add(gameDto);
            }

            return Ok(gamesDtoList);
        }

        [HttpPost("CreateGame")]
        [Authorize]
        public IActionResult CreateGame([FromBody] CreateGameDTO gameData)
        {
            var createdGame = _availableGamesServices.CreateGame(gameData.Name, gameData.Status, gameData.PlayerCount, gameData.GameType, gameData.PricePerSquare, gameData.DailySportsGameId);
           var dataSaved = _generalServices.SaveData(createdGame);
            if (!dataSaved)
            {
                return BadRequest("Failed to save game data.");
            }
            _sportsGameServices.SetGameInUse(gameData.DailySportsGameId);
            var gameDto = _mapperHelpers.AvailableGamesMapper(createdGame);
            return Ok(gameDto);
        }
    }
}
