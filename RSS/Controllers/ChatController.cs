using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RSS.DTOs;
using RSS_Services;
using System.Security.Claims;

namespace RSS.Controllers
{
    [ApiController]
    [Route("SquareGames/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatServices _chatServices;

        public ChatController(ChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        [HttpPost("{gameId}")]
        [Authorize]
        [EnableRateLimiting("chat-send")]
        public async Task<IActionResult> SendMessage(string gameId, [FromBody] SendChatMessageDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var message = await _chatServices.SendMessage(userId, gameId, dto.Message);
                return Ok(message);
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

        [HttpGet("{gameId}")]
        [Authorize]
        public async Task<IActionResult> GetRecentMessages(string gameId)
        {
            try
            {
                var messages = await _chatServices.GetRecentMessages(gameId);
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
