using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace AIATC.ScenarioService.Services;

public interface IAviationWeatherService
{
    Task<MetarData?> GetMetarAsync(string icaoCode);
}

public class AviationWeatherService : IAviationWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public AviationWeatherService(HttpClient httpClient, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<MetarData?> GetMetarAsync(string icaoCode)
    {
        if (string.IsNullOrWhiteSpace(icaoCode)) return null;

        var code = icaoCode.Trim().ToUpperInvariant();
        var cacheKey = $"metar:{code}";

        if (_memoryCache.TryGetValue<MetarData>(cacheKey, out var cached) && cached != null)
        {
            Log.Debug("Returning cached METAR for {Airport}", code);
            return cached;
        }

        try
        {
            var url = $"https://aviationweather.gov/api/data/metar?ids={Uri.EscapeDataString(code)}&format=json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("METAR request failed for {Airport}: {Status}", code, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<MetarData>>(json, _jsonOptions);
            var metar = items?.FirstOrDefault();

            if (metar != null)
            {
                _memoryCache.Set(cacheKey, metar, CacheDuration);
                Log.Information("Retrieved METAR for {Airport}: {FlightCat} wind {Wdir}@{Wspd}kts vis {Vis}",
                    code, metar.FltCat, metar.Wdir, metar.Wspd, metar.Visib);
            }
            else
            {
                Log.Warning("No METAR data returned for {Airport}", code);
            }

            return metar;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching METAR for {Airport}", code);
            return null;
        }
    }
}

public class MetarData
{
    [JsonPropertyName("icaoId")]
    public string? IcaoId { get; set; }

    [JsonPropertyName("rawOb")]
    public string? RawOb { get; set; }

    [JsonPropertyName("reportTime")]
    public string? ReportTime { get; set; }

    [JsonPropertyName("temp")]
    public double? Temp { get; set; }

    [JsonPropertyName("dewp")]
    public double? Dewp { get; set; }

    [JsonPropertyName("wdir")]
    public int? Wdir { get; set; }

    [JsonPropertyName("wspd")]
    public int? Wspd { get; set; }

    [JsonPropertyName("wgst")]
    public int? Wgst { get; set; }

    [JsonPropertyName("visib")]
    public string? Visib { get; set; }

    [JsonPropertyName("altim")]
    public double? Altim { get; set; }

    [JsonPropertyName("cover")]
    public string? Cover { get; set; }

    [JsonPropertyName("clouds")]
    public List<MetarCloud>? Clouds { get; set; }

    [JsonPropertyName("fltCat")]
    public string? FltCat { get; set; }
}

public class MetarCloud
{
    [JsonPropertyName("cover")]
    public string? Cover { get; set; }

    [JsonPropertyName("base")]
    public int? Base { get; set; }
}
