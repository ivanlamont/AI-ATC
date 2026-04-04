using System.Text.Json;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIATC.BFF.Controllers;

[ApiController]
[Route("api/speech")]
public class SpeechController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AzureSpeechBffOptions> _options;
    private readonly IOptions<PiperTtsBffOptions> _piperOptions;
    private readonly SpeechTokenCache _cache;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureSpeechBffOptions> options,
        IOptions<PiperTtsBffOptions> piperOptions,
        SpeechTokenCache cache,
        ILogger<SpeechController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _piperOptions = piperOptions;
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

    /// <summary>
    /// Proxies a TTS synthesis request to the Piper HTTP API.
    /// The WASM client cannot reach the Piper container directly.
    /// Piper expects POST / with JSON {"text": "...", "voice": "..."}.
    /// </summary>
    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize([FromQuery] string voice)
    {
        var piperBase = _piperOptions.Value.BaseUrl.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(piperBase))
        {
            _logger.LogWarning("Piper TTS base URL is not configured");
            return StatusCode(503, new { error = "Piper TTS not configured" });
        }

        using var reader = new StreamReader(Request.Body);
        var text = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "Text body is required" });

        try
        {
            var client = _httpClientFactory.CreateClient("piper-tts");

            // Piper's built-in Flask server expects POST / with JSON body
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                text,
                voice
            });

            var piperRequest = new HttpRequestMessage(HttpMethod.Post, piperBase)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(piperRequest);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Piper TTS synthesis failed ({Status}): {Body}",
                    response.StatusCode, body);
                return StatusCode((int)response.StatusCode,
                    new { error = "Piper TTS synthesis failed" });
            }

            var audio = await response.Content.ReadAsByteArrayAsync();
            return File(audio, "audio/wav");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception proxying to Piper TTS");
            return StatusCode(502, new { error = "Failed to reach Piper TTS" });
        }
    }

    /// <summary>
    /// Health check for the Piper TTS backend.
    /// </summary>
    [HttpGet("piper-status")]
    public async Task<IActionResult> PiperStatus()
    {
        var piperBase = _piperOptions.Value.BaseUrl.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(piperBase))
            return StatusCode(503, new { available = false });

        try
        {
            var client = _httpClientFactory.CreateClient("piper-tts");
            var response = await client.GetAsync($"{piperBase}/voices");
            return Ok(new { available = response.IsSuccessStatusCode });
        }
        catch
        {
            return Ok(new { available = false });
        }
    }
}
