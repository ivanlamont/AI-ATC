using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIATC.BFF.Controllers;

[ApiController]
[Route("api/flights")]
[Authorize(AuthenticationSchemes = "AiatcCookie")]
public class FlightsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<FlightAwareBffOptions> _options;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(
        IHttpClientFactory httpClientFactory,
        IOptions<FlightAwareBffOptions> options,
        ILogger<FlightsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Proxies a flight-search query to FlightAware Aero API.
    /// The API key is added server-side and is never visible in the browser.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("query is required");

        var client = CreateFlightAwareClient();
        try
        {
            var response = await client.GetAsync(
                $"/aeroapi/flights/search?query={Uri.EscapeDataString(query)}");

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FlightAware search failed ({Status}): {Body}",
                    response.StatusCode, body);
            }

            return Content(body, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying FlightAware search");
            return StatusCode(502, new { error = "FlightAware upstream error" });
        }
    }

    /// <summary>
    /// Proxies an airport flights request to FlightAware Aero API.
    /// </summary>
    [HttpGet("airport/{code}")]
    public async Task<IActionResult> Airport(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("airport code is required");

        var client = CreateFlightAwareClient();
        try
        {
            var response = await client.GetAsync(
                $"/aeroapi/airports/{Uri.EscapeDataString(code)}/flights");

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FlightAware airport lookup failed ({Status}): {Body}",
                    response.StatusCode, body);
            }

            return Content(body, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying FlightAware airport lookup for {Code}", code);
            return StatusCode(502, new { error = "FlightAware upstream error" });
        }
    }

    private HttpClient CreateFlightAwareClient()
    {
        var client = _httpClientFactory.CreateClient("flightaware");
        client.DefaultRequestHeaders.Remove("x-apikey");
        client.DefaultRequestHeaders.Add("x-apikey", _options.Value.ApiKey);
        client.DefaultRequestHeaders.Remove("Accept");
        client.DefaultRequestHeaders.Add("Accept", "application/json; charset=UTF-8");
        return client;
    }
}
