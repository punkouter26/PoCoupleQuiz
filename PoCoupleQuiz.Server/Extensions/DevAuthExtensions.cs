using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PoCoupleQuiz.Server.Extensions;

/// <summary>
/// Development-only cookie-based authentication scheme.
/// Allows bypassing Microsoft OAuth for local dev and Playwright E2E tests.
/// Usage: GET /dev-login?name=YourName  →  sets dev_user cookie  →  all [Authorize] endpoints accept it.
/// </summary>
public static class DevAuthExtensions
{
    public const string SchemeName = "DevCookieAuth";

    public static AuthenticationBuilder AddDevAuth(this IServiceCollection services)
    {
        return services
            .AddAuthentication(SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(SchemeName, null);
    }
}

internal sealed class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue("dev_user", out var userName)
            || string.IsNullOrWhiteSpace(userName))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.Name,              userName),
            new Claim("name",                        userName),
            new Claim("preferred_username",          $"{userName}@dev.local"),
            new Claim(ClaimTypes.NameIdentifier,    $"dev-{userName.ToLowerInvariant()}"),
        };
        var identity  = new ClaimsIdentity(claims, DevAuthExtensions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, DevAuthExtensions.SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
