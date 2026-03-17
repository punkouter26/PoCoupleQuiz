using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using System.Security.Claims;

namespace PoCoupleQuiz.Client.Services;

/// <summary>
/// Development-only AuthenticationStateProvider driven by the <c>?user=Name</c> query parameter.
/// Usage:
///   /?user=Alice   → authenticated as Alice in the current browser window<br/>
///   Incognito /?user=Bob  → authenticated as Bob (separate cookie jar)<br/>
/// 
/// On reading a <c>?user=</c> param, silently POSTs to <c>/api/dev/set-identity</c> so the
/// server-side <see cref="DevAuthHandler"/> cookie is kept in sync for SignalR + HTTP API calls.
/// Falls back to <c>/api/dev-auth/state</c> (cookie-based) when navigating within the SPA
/// after the URL no longer contains <c>?user=</c>.
/// </summary>
public sealed class TestAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly HttpClient _http;
    private readonly NavigationManager _nav;
    private string? _currentUser;

    public TestAuthStateProvider(HttpClient http, NavigationManager nav)
    {
        _http = http;
        _nav  = nav;
        _nav.LocationChanged += OnLocationChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var userName = ExtractUser(_nav.Uri);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            if (userName != _currentUser)
            {
                _currentUser = userName;
                await SyncCookie(userName);         // keep server cookie aligned
            }
            return BuildState(userName);
        }

        // No ?user= in URL — fall back to cookie already set by a prior navigation
        try
        {
            var user = await _http.GetFromJsonAsync<DevUser>("/api/dev-auth/state");
            if (!string.IsNullOrEmpty(user?.Name))
            {
                _currentUser = user.Name;
                return BuildState(user.Name);
            }
        }
        catch { /* server unreachable during startup */ }

        _currentUser = null;
        return Anonymous;
    }

    // Fires on every client-side navigation — detects ?user= changes
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var userName = ExtractUser(e.Location);
        if (!string.IsNullOrWhiteSpace(userName) && userName != _currentUser)
        {
            _currentUser = userName;
            // Fire-and-forget: set cookie in background, then update blazor auth state
            _ = Task.Run(async () =>
            {
                await SyncCookie(userName);
                NotifyAuthenticationStateChanged(Task.FromResult(BuildState(userName)));
            });
        }
    }

    /// <summary>POST to server to set <c>dev_user</c> cookie so API + SignalR auth works.</summary>
    private async Task SyncCookie(string userName)
    {
        try
        {
            await _http.PostAsync(
                $"/api/dev/set-identity?name={Uri.EscapeDataString(userName)}", content: null);
        }
        catch { /* best-effort; hub connection delay is acceptable */ }
    }

    /// <summary>Extracts the <c>?user=</c> query param from a URI string.</summary>
    private static string? ExtractUser(string uri)
    {
        var q = uri.IndexOf('?');
        if (q < 0) return null;
        foreach (var segment in uri[(q + 1)..].Split('&'))
        {
            var eq = segment.IndexOf('=');
            if (eq < 0) continue;
            if (segment[..eq].Equals("user", StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(segment[(eq + 1)..]);
        }
        return null;
    }

    private static AuthenticationState BuildState(string name) =>
        new(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name,           name),
            new Claim("name",                     name),
            new Claim(ClaimTypes.NameIdentifier, $"dev-{name.ToLowerInvariant()}"),
        ], "TestAuth")));

    public void Dispose() => _nav.LocationChanged -= OnLocationChanged;

    private record DevUser(string? Name);
}
