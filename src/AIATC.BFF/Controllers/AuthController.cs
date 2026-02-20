using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIATC.BFF.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private const string SessionKeyPkce = "pkce";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<BffOAuthOptions> _options;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IOptions<BffOAuthOptions> options,
        ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Starts the OAuth flow. Generates PKCE, stores state in server session,
    /// and redirects the browser to the identity provider.
    /// </summary>
    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider, [FromQuery] string returnUrl = "/")
    {
        if (!IsValidProvider(provider, out var providerOptions))
            return BadRequest($"Unknown provider: {provider}");

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = Guid.NewGuid().ToString("N");

        // Persist PKCE state in the server-side session (never touches the browser)
        var pkce = new PkceState
        {
            CodeVerifier = codeVerifier,
            State = state,
            Provider = provider,
            ReturnUrl = returnUrl
        };
        HttpContext.Session.SetString(SessionKeyPkce, JsonSerializer.Serialize(pkce));

        var redirectUri = BuildRedirectUri();
        var authUrl = BuildAuthorizationUrl(provider, providerOptions, redirectUri, state, codeChallenge);

        return Redirect(authUrl);
    }

    /// <summary>
    /// OAuth callback. Validates state, exchanges the code server-side (with
    /// client_secret + code_verifier), signs the user in via HttpOnly cookie,
    /// then redirects to the original return URL.
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("OAuth callback returned error: {Error}", error);
            return Redirect("/?auth_error=" + Uri.EscapeDataString(error));
        }

        var pkceJson = HttpContext.Session.GetString(SessionKeyPkce);
        if (string.IsNullOrWhiteSpace(pkceJson))
        {
            _logger.LogWarning("OAuth callback: no PKCE state in session");
            return Redirect("/?auth_error=session_missing");
        }

        PkceState pkce;
        try { pkce = JsonSerializer.Deserialize<PkceState>(pkceJson)!; }
        catch
        {
            _logger.LogWarning("OAuth callback: corrupt PKCE session");
            return Redirect("/?auth_error=session_corrupt");
        }

        // One-time use — clear before any await to prevent replay
        HttpContext.Session.Remove(SessionKeyPkce);

        if (!string.Equals(state, pkce.State, StringComparison.Ordinal))
        {
            _logger.LogWarning("OAuth callback: state mismatch");
            return Redirect("/?auth_error=state_mismatch");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("OAuth callback: missing code");
            return Redirect("/?auth_error=missing_code");
        }

        if (!IsValidProvider(pkce.Provider, out var providerOptions))
            return Redirect("/?auth_error=unknown_provider");

        try
        {
            var redirectUri = BuildRedirectUri();
            var tokenResponse = await ExchangeCodeAsync(
                pkce.Provider, providerOptions, code, redirectUri, pkce.CodeVerifier);

            var claims = ParseJwtClaims(tokenResponse.IdToken ?? tokenResponse.AccessToken);
            var subject = claims.GetValueOrDefault("sub")
                       ?? claims.GetValueOrDefault("oid")
                       ?? Guid.NewGuid().ToString("N");
            var displayName = claims.GetValueOrDefault("name")
                           ?? claims.GetValueOrDefault("given_name")
                           ?? claims.GetValueOrDefault("preferred_username")
                           ?? $"{pkce.Provider} User";
            var email = claims.GetValueOrDefault("email")
                     ?? claims.GetValueOrDefault("preferred_username");

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("provider", pkce.Provider),
                new Claim(ClaimTypes.NameIdentifier, $"{pkce.Provider}:{subject}"),
                new Claim(ClaimTypes.Name, displayName),
                new Claim("access_token", tokenResponse.AccessToken),
            }, "AiatcCookie");

            if (email != null)
                identity.AddClaim(new Claim(ClaimTypes.Email, email));

            await HttpContext.SignInAsync("AiatcCookie", new ClaimsPrincipal(identity));

            var returnUrl = IsLocalUrl(pkce.ReturnUrl) ? pkce.ReturnUrl : "/";
            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth code exchange failed for provider {Provider}", pkce.Provider);
            return Redirect("/?auth_error=exchange_failed");
        }
    }

    /// <summary>
    /// Returns the currently authenticated user's info. No tokens are exposed.
    /// </summary>
    [Authorize(AuthenticationSchemes = "AiatcCookie")]
    [HttpGet("userinfo")]
    public IActionResult UserInfo()
    {
        var principal = User;
        return Ok(new UserInfoResponse
        {
            Provider = principal.FindFirstValue("provider") ?? string.Empty,
            UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = principal.FindFirstValue(ClaimTypes.Email)
        });
    }

    /// <summary>Clears the authentication cookie.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AiatcCookie");
        return Ok();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private bool IsValidProvider(string provider, out OAuthProviderBffOptions opts)
    {
        var o = _options.Value;
        if (provider.Equals("azure", StringComparison.OrdinalIgnoreCase))
        {
            opts = o.Azure;
            return !string.IsNullOrWhiteSpace(opts.ClientId);
        }
        if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
        {
            opts = o.Google;
            return !string.IsNullOrWhiteSpace(opts.ClientId);
        }
        opts = new OAuthProviderBffOptions();
        return false;
    }

    private string BuildRedirectUri() =>
        $"{Request.Scheme}://{Request.Host}/auth/callback";

    private static string BuildAuthorizationUrl(
        string provider,
        OAuthProviderBffOptions opts,
        string redirectUri,
        string state,
        string codeChallenge)
    {
        var (authEndpoint, scope) = provider.Equals("azure", StringComparison.OrdinalIgnoreCase)
            ? ($"{opts.Authority.TrimEnd('/')}/oauth2/v2.0/authorize", opts.Scope)
            : ("https://accounts.google.com/o/oauth2/v2/auth", opts.Scope);

        return $"{authEndpoint}" +
               $"?response_type=code" +
               $"&client_id={Uri.EscapeDataString(opts.ClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&state={Uri.EscapeDataString(state)}" +
               $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
               "&code_challenge_method=S256";
    }

    private async Task<TokenResponse> ExchangeCodeAsync(
        string provider,
        OAuthProviderBffOptions opts,
        string code,
        string redirectUri,
        string codeVerifier)
    {
        var tokenEndpoint = provider.Equals("azure", StringComparison.OrdinalIgnoreCase)
            ? $"{opts.Authority.TrimEnd('/')}/oauth2/v2.0/token"
            : "https://oauth2.googleapis.com/token";

        var clientName = provider.Equals("azure", StringComparison.OrdinalIgnoreCase)
            ? "azure-oauth"
            : "google-oauth";

        var client = _httpClientFactory.CreateClient(clientName);
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = opts.ClientId,
            ["client_secret"] = opts.ClientSecret,
            ["code_verifier"] = codeVerifier
        };

        var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Token exchange failed ({response.StatusCode}): {body}");

        var token = JsonSerializer.Deserialize<TokenResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            throw new InvalidOperationException("Token response did not contain an access_token");

        return token;
    }

    private static Dictionary<string, string> ParseJwtClaims(string? jwt)
    {
        var claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(jwt)) return claims;

        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                claims[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? string.Empty
                    : prop.Value.ToString();
            }
        }
        catch { /* malformed JWT — return empty claims */ }

        return claims;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static string GenerateCodeChallenge(string verifier)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(verifier));
        return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private bool IsLocalUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url) && Url.IsLocalUrl(url);

    // ── Inner types ──────────────────────────────────────────────────────────

    private sealed class PkceState
    {
        public string CodeVerifier { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = "/";
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
