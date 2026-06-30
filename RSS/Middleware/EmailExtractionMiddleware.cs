using System.Security.Claims;
using System.Text.Json;

namespace RSS.Middleware;

public class EmailExtractionMiddleware
{
    private readonly RequestDelegate _next;

    public EmailExtractionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        if (path.Equals("/auth/forgot-password", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/reset-password", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.EnableBuffering();
            try
            {
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("email", out var emailEl))
                    context.Items["rate-limit-key"] = emailEl.GetString()?.ToLowerInvariant();
            }
            catch { /* body unreadable — rate limiter falls back to unknown key */ }
        }
        else if (path.Equals("/user/request-email-change", StringComparison.OrdinalIgnoreCase))
        {
            // Authenticated endpoint — user ID is already available from JWT claims
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
                context.Items["rate-limit-key"] = userId;
        }

        await _next(context);
    }
}
