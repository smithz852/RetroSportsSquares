using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using RSS_DB.Entities;
using RSS_Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RSS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly CoinWalletService _coinWalletService;
        private readonly ILogger<AuthController> _logger;

        // Version of the Terms of Service / Privacy Policy presented at signup.
        // Bump this if the legal text changes materially, so AcceptedTermsAt/TermsVersion
        // can later be used to detect stale consent.
        private const string CurrentTermsVersion = "2026-11-03";
        private const int MinimumAge = 18;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, TokenService tokenService, IMemoryCache cache, CoinWalletService coinWalletService, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _tokenService = tokenService;
            _cache = cache;
            _coinWalletService = coinWalletService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var coinBalance = await _coinWalletService.EnsureDailyGrantAsync(user.Id);
            return Ok(new { token, user = new { user.Id, user.Email, user.DisplayName, user.GamerTag, IsAdmin = roles.Contains("Admin"), CoinBalance = coinBalance, user.HasSeenWelcome } });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            // The daily coin grant rides on /me since it runs on every app load;
            // idempotent per PST day, so repeated calls are cheap no-ops.
            var coinBalance = await _coinWalletService.EnsureDailyGrantAsync(user.Id);
            return Ok(new { user.Id, user.Email, user.DisplayName, user.GamerTag, IsAdmin = roles.Contains("Admin"), CoinBalance = coinBalance, user.HasSeenWelcome });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            // Rotating the security stamp invalidates every outstanding JWT for
            // this user ("log out everywhere"); evict the cached stamp so the
            // revocation takes effect immediately instead of after the 30s TTL
            await _userManager.UpdateSecurityStampAsync(user);
            _cache.Remove($"stamp_{userId}");

            return Ok(new { message = "Logged out." });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists" });

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.DateOfBirth > today)
                return BadRequest(new { message = "Invalid date of birth." });

            var age = today.Year - dto.DateOfBirth.Year;
            if (dto.DateOfBirth > today.AddYears(-age))
                age--;

            if (age < MinimumAge)
                return BadRequest(new { message = "You must be 18 or older to create an account." });

            if (!dto.AgeConfirmed || !dto.AcceptedTerms)
                return BadRequest(new { message = "You must confirm you are 18+ and accept the Terms of Service." });

            var now = DateTimeOffset.UtcNow;
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = dto.Name,
                GamerTag = dto.GamerTag?.Trim().ToUpper(),
                AgeVerified = true,
                AgeVerifiedAt = now,
                AcceptedTermsAt = now,
                TermsVersion = CurrentTermsVersion
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Audit trail for the age gate without persisting the raw date of birth (data minimization) —
            // birth year + computed age is enough to reconstruct why AgeVerified was set true.
            _logger.LogInformation("Signup age gate passed: user {Email}, birth year {BirthYear}, computed age {Age}", dto.Email, dto.DateOfBirth.Year, age);

            return Ok(new { message = "User created successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // TODO: rate limit to 3 requests per email per hour (Phase 6)
            await _tokenService.SendPasswordResetEmailAsync(dto.Email);
            return Ok(new { message = "If that email is registered, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid request." });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password reset successfully." });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("security_stamp", user.SecurityStamp!)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Admin tokens carry elevated access, so keep their lifetime short
            var expires = roles.Contains("Admin")
                ? DateTime.Now.AddHours(24)
                : DateTime.Now.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterDto(string Email, string Password, string Name, string? GamerTag, DateOnly DateOfBirth, bool AgeConfirmed, bool AcceptedTerms);
    public record LoginDto(string Email, string Password);
    public record ForgotPasswordDto(string Email);
    public record ResetPasswordDto(string Email, string Token, string NewPassword);
}