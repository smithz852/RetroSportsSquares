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
        private readonly AccountDeletionService _accountDeletionService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserServices userServices, UserManager<ApplicationUser> userManager, TokenService tokenService, AccountDeletionService accountDeletionService, ILogger<UserController> logger)
        {
            _userServices = userServices;
            _userManager = userManager;
            _tokenService = tokenService;
            _accountDeletionService = accountDeletionService;
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

        [HttpPatch("welcome-seen")]
        public async Task<IActionResult> MarkWelcomeSeen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _userServices.MarkWelcomeSeenAsync(userId);
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

        [HttpPatch("email-preferences")]
        public async Task<IActionResult> UpdateEmailPreferences([FromBody] UpdateEmailPreferencesDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            user.RecapEmailsOptedOut = dto.RecapEmailsOptedOut;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        // Unauthenticated by design — CAN-SPAM forbids requiring login to opt out.
        // The token (not the user ID alone) is what authorizes the change.
        [AllowAnonymous]
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user != null && user.UnsubscribeToken != null && user.UnsubscribeToken == dto.Token)
            {
                user.RecapEmailsOptedOut = true;
                await _userManager.UpdateAsync(user);
            }

            // Same response either way — don't reveal whether the id/token was valid.
            return Ok(new { message = "You will no longer receive game recap emails." });
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            if (dto.ConfirmText != "DELETE")
                return BadRequest(new { message = "Type DELETE to confirm." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordValid) return Unauthorized();

            try
            {
                await _accountDeletionService.DeleteAccountAsync(user);
                return Ok(new { message = "Your account has been deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account for user {UserId}", userId);
                return StatusCode(500, new { message = "Failed to delete account. Please try again or contact support." });
            }
        }
    }

    public record UpdateDisplayNameDto(string DisplayName);
    public record UpdateGamerTagDto(string GamerTag);
    public record RequestEmailChangeDto(string NewEmail, string CurrentPassword);
    public record ConfirmEmailChangeDto(string NewEmail, string Token);
    public record DeleteAccountDto(string CurrentPassword, string ConfirmText);
    public record UpdateEmailPreferencesDto(bool RecapEmailsOptedOut);
    public record UnsubscribeDto(string UserId, Guid Token);
}
