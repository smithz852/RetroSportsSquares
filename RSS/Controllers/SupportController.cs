using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RSS.DTOs;
using RSS_Services;

namespace RSS.Controllers
{
    [ApiController]
    [Route("Support")]
    public class SupportController : ControllerBase
    {
        private readonly SupportService _supportService;

        public SupportController(SupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpPost("send")]
        [EnableRateLimiting("support-send")]
        public async Task<IActionResult> Send([FromBody] SupportRequestDTO dto)
        {
            // Honeypot: real users never fill this hidden field, so a filled value means a bot.
            // Return the normal success response so the bot gains no signal.
            if (!string.IsNullOrEmpty(dto.Website))
                return Ok(new { message = "Thanks! We'll follow up if needed." });

            try
            {
                await _supportService.SendSupportRequestAsync(dto.Name, dto.Email, dto.Category, dto.Message);
                return Ok(new { message = "Thanks! We'll follow up if needed." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { message = "Unable to send your message right now. Please try again later." });
            }
        }
    }
}
