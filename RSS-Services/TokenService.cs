using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RSS_DB.Entities;
using ZlEmailProvider;

namespace RSS_Services;

public class TokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<TokenService> _logger;

    public TokenService(UserManager<ApplicationUser> userManager, IEmailService emailService, ILogger<TokenService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return; // Don't reveal whether the email exists

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        await _emailService.SendPasswordResetAsync(user.Email!, encodedToken);
        _logger.LogInformation("Password reset email sent to {Email}", user.Email);
    }

    public async Task SendEmailVerificationAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        await _emailService.SendEmailVerificationAsync(user.Email!, encodedToken);
        _logger.LogInformation("Verification email sent to {Email}", user.Email);
    }

    public async Task SendEmailChangeConfirmationAsync(ApplicationUser user, string newEmail)
    {
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var encodedToken = Uri.EscapeDataString(token);

        // Critical — let this throw if it fails so the caller can surface the error
        await _emailService.SendEmailChangeConfirmationAsync(newEmail, newEmail, encodedToken);
        _logger.LogInformation("Email change confirmation sent to {NewEmail} for user {UserId}", newEmail, user.Id);

        // Best-effort — don't block the user if the notice fails
        try
        {
            await _emailService.SendEmailChangeNoticeAsync(user.Email!);
            _logger.LogInformation("Email change notice sent to {OldEmail} for user {UserId}", user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email change notice to {OldEmail} for user {UserId}", user.Email, user.Id);
        }
    }
}
