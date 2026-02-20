using System.Text.Json;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIATC.BFF.Controllers;

[ApiController]
[Route("api/speech")]
[Authorize(AuthenticationSchemes = "AiatcCookie")]
public class SpeechController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AzureSpeechBffOptions> _options;
    private readonly SpeechTokenCache _cache;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureSpeechBffOptions> options,
        SpeechTokenCache cache,
        ILogger<SpeechController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Issues a short-lived Azure Speech token (9-min cache, 10-min Azure lifetime).
    /// The browser uses this token with Authorization: Bearer for direct Azure Speech calls.
    /// The subscription key is never sent to the browser.
    /// </summary>
    [HttpPost("token")]
    public async Task<IActionResult> Token()
    {
        var opts = _options.Value;
        if (string.IsNullOrWhiteSpace(opts.SubscriptionKey))
        {
            _logger.LogWarning("Azure Speech subscription key is not configured in BFF");
            return StatusCode(503, new { error = "Speech service not configured" });
        }

        await _cache.Lock.WaitAsync();
        try
        {
            if (!_cache.IsExpired)
            {
                return Ok(new { token = _cache.Token, region = opts.Region });
            }

            // Fetch a fresh token from Azure
            var client = _httpClientFactory.CreateClient("azure-speech");
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://{opts.Region}.api.cognitive.microsoft.com/sts/v1.0/issueToken");
            request.Headers.Add("Ocp-Apim-Subscription-Key", opts.SubscriptionKey);
            request.Content = new StringContent(string.Empty);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Speech token issuance failed ({Status}): {Body}",
                    response.StatusCode, body);
                return StatusCode((int)response.StatusCode, new { error = "Failed to obtain speech token" });
            }

            var token = await response.Content.ReadAsStringAsync();
            _cache.Set(token);

            _logger.LogInformation("Azure Speech token refreshed for region {Region}", opts.Region);
            return Ok(new { token, region = opts.Region });
        }
        finally
        {
            _cache.Lock.Release();
        }
    }
}
