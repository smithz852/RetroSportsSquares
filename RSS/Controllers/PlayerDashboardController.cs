using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS_Services;
using System.Security.Claims;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PlayerDashboardController : ControllerBase
    {
        private readonly PlayerDashboardService _dashboardService;

        public PlayerDashboardController(PlayerDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var stats = await _dashboardService.GetStatsAsync(userId);
            return Ok(stats);
        }

        [HttpGet("current-games")]
        public async Task<IActionResult> GetCurrentGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var games = await _dashboardService.GetCurrentGamesAsync(userId);
            return Ok(games);
        }

        [HttpGet("past-games")]
        public async Task<IActionResult> GetPastGames([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _dashboardService.GetPastGamesAsync(userId, page, pageSize);
            return Ok(result);
        }
    }
}
