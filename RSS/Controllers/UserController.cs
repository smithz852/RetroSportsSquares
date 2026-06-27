using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSS_Services;
using System.Security.Claims;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserServices _userServices;

        public UserController(UserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPatch("display-name")]
        public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
                return BadRequest(new { message = "Display name cannot be empty." });

            var result = await _userServices.UpdateDisplayNameAsync(userId, dto.DisplayName.Trim());
            if (!result.Succeeded)
                return BadRequest(new { message = result.Errors.FirstOrDefault()?.Description });

            return NoContent();
        }

        [HttpPatch("gamer-tag")]
        public async Task<IActionResult> UpdateGamerTag([FromBody] UpdateGamerTagDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tag = dto.GamerTag?.Trim();
            if (string.IsNullOrEmpty(tag) || tag.Length < 2 || tag.Length > 5)
                return BadRequest(new { message = "Gamer tag must be 2-5 characters." });

            var result = await _userServices.UpdateGamerTagAsync(userId, tag.ToUpper());
            if (!result.Succeeded)
                return BadRequest(new { message = result.Errors.FirstOrDefault()?.Description });

            return NoContent();
        }
    }

    public record UpdateDisplayNameDto(string DisplayName);
    public record UpdateGamerTagDto(string GamerTag);
}
