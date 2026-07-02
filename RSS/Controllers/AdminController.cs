using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS_Services;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminDashboardService _adminService;

        public AdminController(AdminDashboardService adminService)
        {
            _adminService = adminService;
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
    }
}
