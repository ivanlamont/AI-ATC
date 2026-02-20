using System.Text.Json;
using Microsoft.JSInterop;
using Serilog;

namespace AIATC.Web.Services;

public class AuthUser
{
    public string Provider { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public interface IAuthService
{
    bool IsAuthenticated { get; }
    AuthUser? CurrentUser { get; }
    event Action? AuthStateChanged;
    Task InitializeAsync();
    Task LoginWithAzureAsync();
    Task LoginWithGoogleAsync();
    Task LogoutAsync();
}

/// <summary>
/// BFF-backed auth service. All OAuth logic runs server-side in AIATC.BFF;
/// the browser only ever holds an HttpOnly cookie — no tokens, no client secrets.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;

    public AuthUser? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action? AuthStateChanged;

    public AuthService(IJSRuntime js, HttpClient httpClient)
    {
        _js = js;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Calls GET /auth/userinfo on the BFF (cookie sent automatically, same origin).
    /// If the cookie is present and valid, CurrentUser is populated.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/auth/userinfo");
            if (!response.IsSuccessStatusCode)
            {
                CurrentUser = null;
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            CurrentUser = JsonSerializer.Deserialize<AuthUser>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (CurrentUser != null)
                AuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize auth state from BFF");
            CurrentUser = null;
        }
    }

    /// <summary>Navigates to /auth/login/azure — the BFF handles the full PKCE flow.</summary>
    public async Task LoginWithAzureAsync() =>
        await _js.InvokeVoidAsync("eval", "window.location.assign('/auth/login/azure?returnUrl=/')");

    /// <summary>Navigates to /auth/login/google — the BFF handles the full PKCE flow.</summary>
    public async Task LoginWithGoogleAsync() =>
        await _js.InvokeVoidAsync("eval", "window.location.assign('/auth/login/google?returnUrl=/')");

    /// <summary>Calls POST /auth/logout on the BFF to clear the HttpOnly cookie.</summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("/auth/logout", null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Logout request failed");
        }
        CurrentUser = null;
        AuthStateChanged?.Invoke();
    }
}
