using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RSS_Services;
using RSS.Helpers;
using RSS.DTOs;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AvailableGamesController : ControllerBase
    {
        private readonly AvailableGamesServices _availableGamesServices;
        private readonly MapperHelpers _mapperHelpers;
        private readonly GeneralServices _generalServices;

        public AvailableGamesController(AvailableGamesServices availableGamesServices, MapperHelpers mapperHelpers, GeneralServices generalServices)
        {
            _availableGamesServices = availableGamesServices;
            _mapperHelpers = mapperHelpers;
            _generalServices = generalServices;

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
        public IActionResult CreateGame([FromBody] CreateGameDTO gameData)
        {
            var createdGame = _availableGamesServices.CreateGame(gameData.Name, gameData.Status, gameData.PlayerCount, gameData.GameType, gameData.PricePerSquare, gameData.DailySportsGameId);
            _generalServices.SaveData(createdGame);
            var gameDto = _mapperHelpers.AvailableGamesMapper(createdGame);
            return Ok(gameDto);
        }
    }
}
