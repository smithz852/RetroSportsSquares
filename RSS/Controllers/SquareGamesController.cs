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
        private readonly IGameHubNotifier _hubNotifier;
        private readonly WalletService _walletService;

        public SquareGamesController(AvailableGamesServices availableGamesServices, MapperHelpers mapperHelpers, GeneralServices generalServices, SportsGameServices sportsGameServices, SquareServices squareServices, AppDbContext appDbContext, GamePlayerServices gamePlayerServices, IGameHubNotifier hubNotifier, WalletService walletService)
        {
            _availableGamesServices = availableGamesServices;
            _mapperHelpers = mapperHelpers;
            _generalServices = generalServices;
            _sportsGameServices = sportsGameServices;
            _squareServices = squareServices;
            _appDbContext = appDbContext;
            _gamePlayerServices = gamePlayerServices;
            _hubNotifier = hubNotifier;
            _walletService = walletService;
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
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDTO gameData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var payoutMode = string.IsNullOrWhiteSpace(gameData.PayoutMode)
                ? RSS_DB.Entities.PayoutModes.Default
                : gameData.PayoutMode;
            if (!RSS_DB.Entities.PayoutModes.All.Contains(payoutMode))
                return BadRequest(new { message = $"Unknown payout mode '{payoutMode}'." });
            if (!RSS_DB.Entities.PayoutModes.Implemented.Contains(payoutMode))
                return BadRequest(new { message = $"Payout mode '{payoutMode}' is coming soon." });

            await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var createdGame = _availableGamesServices.CreateGame(gameData.Name, gameData.IsOpen, gameData.PlayerCount, gameData.GameType, gameData.PricePerSquare, gameData.SquareSelectionLimit, gameData.IsTurnBased, gameData.TurnTimeoutSeconds, gameData.DailySportsGameId, gameData.IsPublic, payoutMode);
                _appDbContext.Set<RSS_DB.Entities.SquareGames>().Add(createdGame);

                await _appDbContext.SaveChangesAsync();

                var createGamePlayerHost = _gamePlayerServices.CreatePlayerHostedGame(userId, createdGame.Id);
                _appDbContext.Set<RSS_DB.Entities.GamePlayer>().Add(createGamePlayerHost);

                _sportsGameServices.SetGameInUse(gameData.DailySportsGameId);

                await _squareServices.GenerateBoardAsync(createdGame.Id.ToString());

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var gameDto = _mapperHelpers.AvailableGamesMapper(createdGame);
                return Ok(gameDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                return BadRequest("Failed to save game data.");
            }
        }

        [HttpPost("start/{gameId}")]
        [Authorize]
        public async Task<IActionResult> StartGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isHost = await _gamePlayerServices.IsPlayerHost(userId, gameId);
            if (!isHost)
                return Forbid();

            var setGameToClosed = await _squareServices.SetGameToClosedById(gameId);
            if (!setGameToClosed)
            {
                return BadRequest("Failed to close game.");
            }

            await _hubNotifier.NotifyGameStarted(gameId);
            return Ok();
        }

        [HttpGet("find/{shortId}")]
        [Authorize]
        public async Task<IActionResult> FindGameByShortId(string shortId)
        {
            var match = _appDbContext.SquareGames
                .Where(g => g.Id.ToString().StartsWith(shortId.ToLower()))
                .Select(g => new { gameId = g.Id.ToString() })
                .FirstOrDefault();

            if (match == null) return NotFound();
            return Ok(match);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSquareGameById(string id)
        {
            var availableGame = await _availableGamesServices.GetGameById(id);
            if (availableGame == null)
            {
                return NotFound();
            }
            var gameDto = _mapperHelpers.AvailableGamesMapper(availableGame);
            return Ok(gameDto);
        }

        [HttpGet("GetSquareGameScoreData/{id}")]
        public async Task<IActionResult> GetGameScoreData(string id)
        {
            var scoreData = _availableGamesServices.GetAllScoreAndWinnerDataByGameId(id);
            if (scoreData == null)
                return NotFound();

            var winnerNames = await _availableGamesServices.GetPeriodWinnerDisplayNames(scoreData.PeriodWinners);
            var payoutPerPeriod = _availableGamesServices.GetPayoutPerPeriod(scoreData);
            var gameDto = _mapperHelpers.ScoreDataMapper(scoreData, winnerNames, payoutPerPeriod);
            return Ok(gameDto);
        }

        [HttpPost("join/{gameId}")]
        [Authorize]
        public async Task<IActionResult> JoinGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var gamePlayer = await _gamePlayerServices.JoinGame(userId, gameId);
                return Ok(new { isHost = gamePlayer.IsHost });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leave/{gameId}")]
        [Authorize]
        public async Task<IActionResult> LeaveGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _gamePlayerServices.LeaveGame(userId, gameId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("begin-selections/{gameId}")]
        [Authorize]
        public async Task<IActionResult> BeginSelections(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isHost = await _gamePlayerServices.IsPlayerHost(userId, gameId);
            if (!isHost) return Forbid();

            try
            {
                await _gamePlayerServices.BeginSelections(gameId);
                var status = await _gamePlayerServices.GetTurnStatus(gameId);
                return Ok(status);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
        }

        [HttpPost("skip-player/{gameId}")]
        [Authorize]
        public async Task<IActionResult> SkipPlayer(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isHost = await _gamePlayerServices.IsPlayerHost(userId, gameId);
            if (!isHost) return Forbid();

            await _gamePlayerServices.AdvanceTurn(gameId);
            var status = await _gamePlayerServices.GetTurnStatus(gameId);
            return Ok(status);
        }

        [HttpGet("turn-status/{gameId}")]
        public async Task<IActionResult> GetTurnStatus(string gameId)
        {
            try
            {
                var status = await _gamePlayerServices.GetTurnStatus(gameId);
                return Ok(status);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
        }

        [HttpPost("SquareSelections/{gameId}")]
        [Authorize]
        public async Task<IActionResult> SelectSquare(string gameId, [FromBody] SquareSelectionDTO squareSelections)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var game = await _availableGamesServices.GetGameById(gameId);
                if (game == null) return NotFound();

                if (game.IsTurnBased && game.SelectionPhaseActive && game.CurrentTurnUserId != userId)
                    return BadRequest(new { message = "It is not your turn." });

                var withinSquareLimit = await _squareServices.SquareLimitCheck(gameId, userId, squareSelections.Selections.Count);
                if (!withinSquareLimit)
                {
                    var squareLimit = await _squareServices.GetSquareSelectionLimit(gameId);
                    return BadRequest(new { message = $"You have exceeded the square selection limit for this game (Limit: {squareLimit})" });
                }

                // Friendly pre-check; the atomic conditional decrement inside
                // CreateSquareSelections is the real double-spend guard.
                if (game.IsPublic)
                {
                    var wagerCost = game.PricePerSquare * squareSelections.Selections.Count;
                    if (!await _walletService.HasSufficientCoinsAsync(userId, wagerCost))
                        return BadRequest(new { message = $"Not enough coins — you need {wagerCost:0.##} coins for this selection." });
                }

                var selectedSquares = await _squareServices.CreateSquareSelections(squareSelections.Selections, userId, gameId);
                if (selectedSquares == null || !selectedSquares.Any())
                    return BadRequest("Failed to save square selection data.");

                if (game.IsTurnBased && game.SelectionPhaseActive)
                    await _gamePlayerServices.AdvanceTurn(gameId);
                else
                    await _hubNotifier.NotifySquareSelected(gameId);

                var squareDtos = selectedSquares.Select(s => _mapperHelpers.SelectedGamePlayerSquaresMapper(s)).ToList();
                return Ok(squareDtos);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
          }


        [HttpGet("GetGameboard/{gameId}")]
        public async Task<IActionResult> GetAllSelectedSquares(string gameId)
        {
            var gameboard = await _squareServices.GetGameboardSquaresByGameId(gameId);
            if (gameboard == null)
            {
                return BadRequest("Gameboard not found");
            }
            var gameboardDto = _mapperHelpers.PreGameboardMapper(gameboard);
            return Ok(gameboardDto);
        }

        [HttpDelete("{gameId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isHost = await _gamePlayerServices.IsPlayerHost(userId, gameId);
            if (!isHost)
                return Forbid();

            var game = await _availableGamesServices.GetGameById(gameId);
            if (game == null)
                return NotFound();

            if (!game.isOpen)
                return BadRequest("Cannot delete a game that has already started.");

            var dailySportGameId = game.DailySportGameId;
            var deleted = await _availableGamesServices.DeleteGame(gameId);
            if (!deleted)
                return BadRequest("Failed to delete game.");

            await _hubNotifier.NotifyGameDeleted(gameId);
            _sportsGameServices.SetGameNotInUse(dailySportGameId);
            return Ok();
        }

        [HttpGet("GetOutsideSquareNumbers/{gameId}")]
        public async Task<IActionResult> GetOutsideSquareNumbers(string gameId)
        {
            var game = await _availableGamesServices.GetGameById(gameId);
            if (game == null)
            {
                return BadRequest("Game not found");
            }
            if (game.isOpen)
            {
                return Ok();
            }

            var outsideSquares = await _squareServices.GetOutsideSquareNumbers(gameId);
            if (outsideSquares == null) return NotFound();
            var outsideSquaresDto = _mapperHelpers.OutsideSquareMapper(outsideSquares);

            return Ok(outsideSquaresDto);
        }

      

    }
}
