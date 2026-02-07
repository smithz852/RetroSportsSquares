using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS.DTOs;
using RSS.Helpers;
using RSS_Services;
using System.Linq;
using System.Security.Claims;

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

        [HttpGet("GetAvailableSquareGames")]
        public IActionResult GetAvailableSquareGames()
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
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return Unauthorized();
            //}
            var createdGame = _availableGamesServices.CreateGame(gameData.Name, gameData.IsOpen, gameData.PlayerCount, gameData.GameType, gameData.PricePerSquare, gameData.DailySportsGameId);
           var dataSaved = _generalServices.SaveData(createdGame);
            if (!dataSaved)
            {
                return BadRequest("Failed to save game data.");
            }
            _sportsGameServices.SetGameInUse(gameData.DailySportsGameId);
            var gameDto = _mapperHelpers.AvailableGamesMapper(createdGame);
            return Ok(gameDto);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetSquareGameById(string id)
        {
            var availableGame = _availableGamesServices.GetGameById(id);
            if (availableGame == null)
            {
                return NotFound();
            }
            var gameDto = _mapperHelpers.AvailableGamesMapper(availableGame);
            return Ok(gameDto);
        }

        [HttpGet("GetSquareGameScoreData/{id}")]
        public IActionResult GetGameScoreData(string id)
        {
            var scoreData = _availableGamesServices.GetAllScoreAndWinnerDataByGameId(id);
            if (scoreData == null)
            {
                return NotFound();
            }
            var gameDto = _mapperHelpers.ScoreDataMapper(scoreData);
            return Ok(gameDto);
        }
    }
}
