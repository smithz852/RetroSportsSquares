using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS.DTOs;
using RSS.Helpers;
using RSS_DB;
using RSS_Services;
using RSS_Services.DTOs;
using RSS_Services.Helpers;
using System.Linq;
using System.Security.Claims;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SquareGamesController : ControllerBase
    {
        private readonly AvailableGamesServices _availableGamesServices;
        private readonly MapperHelpers _mapperHelpers;
        private readonly GeneralServices _generalServices;
        private readonly SportsGameServices _sportsGameServices;
        private readonly SquareServices _squareServices;
        private readonly AppDbContext _appDbContext;
        private readonly GamePlayerServices _gamePlayerServices;

        public SquareGamesController(AvailableGamesServices availableGamesServices, MapperHelpers mapperHelpers, GeneralServices generalServices, SportsGameServices sportsGameServices, SquareServices squareServices, AppDbContext appDbContext, GamePlayerServices gamePlayerServices)
        {
            _availableGamesServices = availableGamesServices;
            _mapperHelpers = mapperHelpers;
            _generalServices = generalServices;
            _sportsGameServices = sportsGameServices;
            _squareServices = squareServices;
            _appDbContext = appDbContext;
            _gamePlayerServices = gamePlayerServices;
        }

        [HttpGet("GetAvailableSquareGames")]
        public IActionResult GetAvailableSquareGames()
        {
            var availableGames = _availableGamesServices.GetAllAvailableGames();
            var gamesDtoList = new List<SquareGamesDTO>();

            if (availableGames == null || !availableGames.Any())
            {
                return Ok(new List<SquareGamesDTO>());
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                var createdGame = _availableGamesServices.CreateGame(gameData.Name, gameData.IsOpen, gameData.PlayerCount, gameData.GameType, gameData.PricePerSquare, gameData.DailySportsGameId);
                _generalServices.SaveData(createdGame);

                var createGamePlayerHost = _gamePlayerServices.CreatePlayerHostedGame(userId, createdGame.Id);
                _generalServices.SaveData(createGamePlayerHost);

                _sportsGameServices.SetGameInUse(gameData.DailySportsGameId);

                transaction.Commit();

                var gameDto = _mapperHelpers.AvailableGamesMapper(createdGame);
                return Ok(gameDto);
            }
            catch
            {
                transaction.Rollback();
                return BadRequest("Failed to save game data.");
            }
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

        [HttpPost("SquareSelections/{gameId}")]
        [Authorize]
        public IActionResult SelectSquare(string gameId, [FromBody] SquareSelectionDTO squareSelections)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unavailableSquares = _squareServices.CheckIfSquaresAreSelected(gameId, squareSelections.Selections);
            if (unavailableSquares.Any())
            {
                return BadRequest(new { message = $"Some squares aren't available, please choose {unavailableSquares.Count} more squares.", unavailableSquares });
            }
            var selectedSquares = _squareServices.CreateSquareSelections(squareSelections.Selections, userId, gameId);
            foreach (var square in selectedSquares)
            {
                var dataSaved = _generalServices.SaveData(square);
                if (!dataSaved)
                {
                    return BadRequest("Failed to save square selection data.");
                }
            }

            var squareDtos = selectedSquares.Select(s => _mapperHelpers.SelectedGamePlayerSquaresMapper(s)).ToList();
            return Ok(squareDtos);
        }
    }
}
