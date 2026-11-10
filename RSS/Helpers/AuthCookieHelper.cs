using Microsoft.AspNetCore.Antiforgery;

namespace RSS.Helpers
{
    // Shared by AuthController (issue/clear on login/logout) and UserController
    // (clear on account deletion / email-change confirmation, which both end the
    // current session the same way logout does).
    //
    // Cookie names use the "__Host-" prefix: the browser itself then refuses to
    // accept the cookie unless it has Secure, Path=/, and no Domain attribute — so
    // a future change that accidentally weakens one of those gets rejected outright
    // instead of silently degrading security.
    public static class AuthCookieHelper
    {
        public const string AuthCookieName = "__Host-token";
        public const string CsrfCookieName = "__Host-XSRF-TOKEN";

        // Antiforgery's own internal "cookie token" name, set via Program.cs's
        // AddAntiforgery(options => options.Cookie.Name = ...). Kept here too so
        // ClearAuthCookies can delete it without duplicating the literal.
        public const string AntiforgeryCookieName = "__Host-af";

        public static void SetAuthCookies(HttpContext httpContext, IAntiforgery antiforgery, string token, DateTime expires)
        {
            var sharedOptions = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = expires
            };

            httpContext.Response.Cookies.Append(AuthCookieName, token, new CookieOptions(sharedOptions) { HttpOnly = true });

            // Antiforgery's own HttpOnly "cookie token" is issued automatically by
            // GetAndStoreTokens (per the Cookie options configured in Program.cs). We
            // only need to separately expose its paired RequestToken via a JS-readable
            // cookie so the frontend can echo it back as the X-XSRF-TOKEN header — see
            // CsrfProtectionMiddleware for the validation side. The incoming /auth/login
            // request is naturally anonymous (there's no cookie yet on *this* request,
            // only the outgoing response), which CsrfProtectionMiddleware deliberately
            // mirrors at validation time to keep both sides consistent — see the comment
            // there for why binding this to the authenticated identity doesn't work.
            var tokenSet = antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append(CsrfCookieName, tokenSet.RequestToken!, new CookieOptions(sharedOptions) { HttpOnly = false });
        }

        public static void ClearAuthCookies(HttpContext httpContext)
        {
            var options = new CookieOptions { Secure = true, SameSite = SameSiteMode.None, Path = "/" };
            httpContext.Response.Cookies.Delete(AuthCookieName, options);
            httpContext.Response.Cookies.Delete(CsrfCookieName, options);
            httpContext.Response.Cookies.Delete(AntiforgeryCookieName, options);
        }
    }
}
