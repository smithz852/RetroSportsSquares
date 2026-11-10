using Microsoft.AspNetCore.Antiforgery;
using RSS.Helpers;
using System.Security.Claims;

namespace RSS.Middleware;

// Double-submit CSRF defense, delegated to ASP.NET Core's own Antiforgery system rather
// than a hand-rolled string compare. The JWT cookie is SameSite=None (required since the
// SPA and API are different sites), so it rides along on *any* cross-site request — CORS
// only stops an attacker from reading the response, not from sending the request. A
// mutating, cookie-authenticated request must also echo the antiforgery RequestToken
// (exposed via the __Host-XSRF-TOKEN cookie, see AuthCookieHelper) back as a header. An
// attacker's page can't read that cookie's value — it belongs to the API's own origin —
// so it can't forge a matching header.
public class CsrfProtectionMiddleware
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    private readonly RequestDelegate _next;

    public CsrfProtectionMiddleware(RequestDelegate next) => _next = next;

    // IAntiforgery is injected via the method, not the constructor, so it resolves from
    // the current request's scope rather than being captured once at startup.
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var hasSessionCookie = context.Request.Cookies.ContainsKey(AuthCookieHelper.AuthCookieName);

        if (!SafeMethods.Contains(context.Request.Method) && hasSessionCookie)
        {
            // Antiforgery binds its token pair to HttpContext.User at the moment it's
            // minted vs. the moment it's validated. AuthCookieHelper mints while the
            // request is naturally anonymous (the auth cookie isn't on the /auth/login
            // request itself, only the response) — but by the time this middleware runs
            // on a later request, UseAuthentication() has already populated context.User
            // from that same cookie, so the two sides would never match. Our CSRF defense
            // doesn't rely on identity-binding anyway (it relies on the attacker being
            // unable to read the cookie cross-origin), so we validate against an
            // anonymous principal too, keeping both sides consistent, and restore the
            // real one afterward for the actual endpoint's [Authorize] checks.
            var realUser = context.User;
            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Missing or invalid CSRF token." });
                return;
            }
            finally
            {
                context.User = realUser;
            }
        }

        await _next(context);
    }
}
