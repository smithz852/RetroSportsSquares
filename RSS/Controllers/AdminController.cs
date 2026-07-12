using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSS.DTOs;
using RSS_DB;
using RSS_Services;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminDashboardService _adminService;
        private readonly ChatServices _chatServices;

        public AdminController(AdminDashboardService adminService, ChatServices chatServices)
        {
            _adminService = adminService;
            _chatServices = chatServices;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _adminService.GetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("games/current")]
        public async Task<IActionResult> GetCurrentGames()
        {
            var games = await _adminService.GetCurrentGamesAsync();
            return Ok(games);
        }

        [HttpGet("games/past")]
        public async Task<IActionResult> GetPastGames([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _adminService.GetPastGamesAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("players/stats")]
        public async Task<IActionResult> GetPlayerStats()
        {
            var stats = await _adminService.GetPlayerStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("games/{gameId}/chat")]
        public async Task<IActionResult> GetGameChatLog(string gameId)
        {
            try
            {
                var messages = await _chatServices.GetChatLogForGame(gameId);
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Dev-only: run the production winner/notification pipeline against a sports game's
        // current DB state. Lets seeded fake games (never present in the live api-sports
        // response) be iterated through quarters without waiting on the refetch automation.
        //[HttpPost("dev/process-winners/{apiGameId:int}")]
        //public async Task<IActionResult> ProcessWinnersFromDb(
        //    int apiGameId,
        //    [FromServices] IHostEnvironment env,
        //    [FromServices] AppDbContext dbContext,
        //    [FromServices] GameResultProcessor resultProcessor)
        //{
        //    if (!env.IsDevelopment()) return NotFound();

        //    var sportsGame = await dbContext.DailySportsGames
        //        .FirstOrDefaultAsync(g => g.ApiGameId == apiGameId);
        //    if (sportsGame == null) return NotFound($"No sports game with ApiGameId {apiGameId}");

        //    var scoreData = new SportScoreUpdateDTO
        //    {
        //        ApiGameId = sportsGame.ApiGameId,
        //        HomeTeamName = sportsGame.HomeTeam,
        //        AwayTeamName = sportsGame.AwayTeam,
        //        Status = sportsGame.Status,
        //        SportType = sportsGame.SportType,
        //        CurrentHomeScore = sportsGame.CurrentHomeScore,
        //        CurrentAwayScore = sportsGame.CurrentAwayScore,
        //        HomePeriodScores = sportsGame.HomePeriodScores,
        //        AwayPeriodScores = sportsGame.AwayPeriodScores
        //    };

        //    await resultProcessor.ProcessQuarterlyWinnersAsync(scoreData, sportsGame.Id);

        //    var squareGames = await dbContext.SquareGames
        //        .Where(g => g.DailySportGameId == sportsGame.Id)
        //        .Select(g => new
        //        {
        //            g.Id,
        //            g.GameName,
        //            g.PeriodWinners,
        //            g.IsCompleted,
        //            g.RecapEmailSent
        //        })
        //        .ToListAsync();

        //    return Ok(new { sportsGame.Status, SquareGames = squareGames });
        //}
    }
}
