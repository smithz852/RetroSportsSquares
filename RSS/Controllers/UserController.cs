using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RSS_DB.Entities;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserServices userServices, UserManager<ApplicationUser> userManager, TokenService tokenService, ILogger<UserController> logger)
        {
            _userServices = userServices;
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
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

        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordValid) return Unauthorized();

            var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
            if (existingUser != null)
                return BadRequest(new { message = "That email address is already in use." });

            try
            {
                await _tokenService.SendEmailChangeConfirmationAsync(user, dto.NewEmail);
                return Ok(new { message = "A confirmation link has been sent to your new email address." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email change confirmation for user {UserId}", user.Id);
                return StatusCode(500, new { message = "Failed to send confirmation email. Please try again or contact support." });
            }
        }

        [HttpPost("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, dto.Token);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.SetUserNameAsync(user, dto.NewEmail);
            return Ok(new { message = "Email address updated successfully." });
        }
    }

    public record UpdateDisplayNameDto(string DisplayName);
    public record UpdateGamerTagDto(string GamerTag);
    public record RequestEmailChangeDto(string NewEmail, string CurrentPassword);
    public record ConfirmEmailChangeDto(string NewEmail, string Token);
}
