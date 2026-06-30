using Microsoft.AspNetCore.Identity;
using RSS_DB.Entities;
using ZlEmailProvider;

namespace RSS_Services;

public class TokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public TokenService(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return; // Don't reveal whether the email exists

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        await _emailService.SendPasswordResetAsync(user.Email!, encodedToken);
    }

    public async Task SendEmailVerificationAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        await _emailService.SendEmailVerificationAsync(user.Email!, encodedToken);
    }

    public async Task SendEmailChangeConfirmationAsync(ApplicationUser user, string newEmail)
    {
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var encodedToken = Uri.EscapeDataString(token);

        // Critical — let this throw if it fails so the caller can surface the error
        await _emailService.SendEmailChangeConfirmationAsync(newEmail, newEmail, encodedToken);

        // Best-effort — don't block the user if the notice fails
        try
        {
            await _emailService.SendEmailChangeNoticeAsync(user.Email!);
        }
        catch
        {
            // TODO (Phase 6): log failure to send security notice to user.Email
        }
    }
}
