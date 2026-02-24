using System.Text.Json;
using System.Text.Json.Serialization;
using AIATC.Domain.Models;
using AIATC.ReferenceData.Context;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace AIATC.ScenarioService.Services;

/// <summary>
/// Service for accessing real-time flight data from FlightAware Aero API.
/// </summary>
public interface IFlightAwareService
{
    Task<List<AircraftModel>> GetLiveFlightsAsync(double centerLat, double centerLon, float radiusNm);
    Task<List<AircraftModel>> GetLiveFlightsForAirportAsync(string airportIcaoCode, float radiusNm = 50.0f);
}

public class FlightAwareOptions
{
    public string? ApiKey { get; set; }
    public int? AirportFlightsCacheMinutes { get; set; }
}

public class FlightAwareService : IFlightAwareService
{
    private readonly HttpClient _httpClient;
    private readonly AirspaceReferenceDbContext _airspaceDb;
    private readonly IMemoryCache _memoryCache;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _apiKey;
    private readonly TimeSpan _airportFlightsCacheDuration;

    public FlightAwareService(
        HttpClient httpClient,
        AirspaceReferenceDbContext airspaceDb,
        IMemoryCache memoryCache,
        IHostEnvironment hostEnvironment,
        IOptions<FlightAwareOptions> options)
    {
        _httpClient = httpClient;
        _airspaceDb = airspaceDb;
        _memoryCache = memoryCache;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var configuredCacheMinutes = options.Value.AirportFlightsCacheMinutes;
        var defaultCacheMinutes = hostEnvironment.IsDevelopment() ? 5 : 20;
        var effectiveCacheMinutes = configuredCacheMinutes.GetValueOrDefault(defaultCacheMinutes);
        _airportFlightsCacheDuration = TimeSpan.FromMinutes(Math.Max(1, effectiveCacheMinutes));

        _apiKey = options.Value.ApiKey ?? string.Empty;
        if (string.IsNullOrEmpty(_apiKey))
        {
            Log.Warning("FlightAware API key not configured. Returning empty live-flight data.");
        }
    }

    public async Task<List<AircraftModel>> GetLiveFlightsForAirportAsync(string airportIcaoCode, float radiusNm)
    {
        var code = AirportReferenceLookup.Normalize(airportIcaoCode);
        if (string.IsNullOrEmpty(code))
        {
            Log.Warning("Airport code was empty for GetLiveFlightsForAirportAsync");
            return [];
        }

        // Try to resolve coordinates from the ARINC 424 reference database.
        // Fall back to well-known hardcoded values when the DB is not provisioned.
        (double Lat, double Lon, string DisplayCode)? coords = null;
        try
        {
            var airport = await AirportReferenceLookup.FindAirportAsync(_airspaceDb, code);
            if (airport != null
                && ArincCoordinateParser.TryParseLatitude(airport.Latitude, out var lat)
                && ArincCoordinateParser.TryParseLongitude(airport.Longitude, out var lon))
            {
                coords = (lat, lon, AirportReferenceLookup.BuildDisplayAirportCode(airport, code));
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Reference database unavailable for airport {Airport}; trying well-known fallback", code);
        }

        if (coords == null)
        {
            var fallback = WellKnownAirports.TryGet(code);
            if (fallback == null)
            {
                Log.Warning("Airport {Airport} not found in reference database or well-known list", code);
                return [];
            }
            coords = (fallback.Latitude, fallback.Longitude, fallback.IcaoCode);
            Log.Information(
                "Using well-known coordinates for airport {Airport} ({Lat}, {Lon})",
                code, fallback.Latitude, fallback.Longitude);
        }

        Log.Information(
            "Resolved airport {Airport} ({Resolved}) to {Latitude}, {Longitude}; requesting live flights in {RadiusNm} NM",
            code, coords.Value.DisplayCode, coords.Value.Lat, coords.Value.Lon, radiusNm);

        var cacheKey = $"flightaware:airport:{coords.Value.DisplayCode}";
        if (_memoryCache.TryGetValue<List<AircraftModel>>(cacheKey, out var cachedFlights) && cachedFlights != null)
        {
            Log.Information("Returning cached live flights for airport {Airport}", coords.Value.DisplayCode);
            return cachedFlights;
        }

        var flights = await GetLiveFlightsAsync(coords.Value.Lat, coords.Value.Lon, radiusNm);
        _memoryCache.Set(cacheKey, flights, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _airportFlightsCacheDuration
        });

        return flights;
    }

    public async Task<List<AircraftModel>> GetLiveFlightsAsync(double centerLat, double centerLon, float radiusNm)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return [];
        }

        var results = new List<AircraftModel>();

        try
        {
            var minLat = centerLat - (radiusNm / 60.0);
            var maxLat = centerLat + (radiusNm / 60.0);
            var minLon = centerLon - (radiusNm / (60.0 * Math.Cos(centerLat * Math.PI / 180.0)));
            var maxLon = centerLon + (radiusNm / (60.0 * Math.Cos(centerLat * Math.PI / 180.0)));
            const int ceiling = 20000;

            Log.Information(
                "Fetching live flights around {Lat}, {Lon} within {RadiusNm} NM",
                centerLat, centerLon, radiusNm);

            var query = $"-belowAltitude {ceiling} -latlong \"{minLat:F6} {minLon:F6} {maxLat:F6} {maxLon:F6}\"";
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"https://aeroapi.flightaware.com/aeroapi/flights/search?query={encodedQuery}&max_pages=1";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json; charset=UTF-8");
            _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to fetch live flights. Status: {StatusCode}", response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync();
            var flightData = JsonSerializer.Deserialize<FlightAwareSearchResponse>(json, _jsonOptions);

            if (flightData?.Flights == null)
            {
                return results;
            }

            results = ConvertToAircraftModels(flightData.Flights, centerLat, centerLon);
            Log.Information("Retrieved {Count} live flights", results.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching live flight data from FlightAware");
        }

        return results;
    }

    private List<AircraftModel> ConvertToAircraftModels(List<FlightAwareFlight> flights, double centerLat, double centerLon)
    {
        var aircraft = new List<AircraftModel>();

        foreach (var flight in flights)
        {
            try
            {
                var position = flight.LastPosition;
                if (position == null)
                {
                    continue;
                }

                var positionNm = LatLonToNm(position.Latitude, position.Longitude, centerLat, centerLon);

                aircraft.Add(new AircraftModel
                {
                    Callsign = flight.Ident ?? "UNKNOWN",
                    PositionNm = positionNm,
                    HeadingRadians = (position.Heading ?? 0) * (float)(Math.PI / 180.0),
                    SpeedKnots = position.Groundspeed,
                    AltitudeFt = position.Altitude * 100, // AeroAPI reports altitude in hundreds of feet
                    MinSpeedKnots = 160,
                    MaxSpeedKnots = 500,
                    AircraftType = string.IsNullOrWhiteSpace(flight.AircraftType) ? "UNK" : flight.AircraftType,
                    IsArrival = false,
                    TargetHeadingRadians = null,
                    TargetAltitudeFt = null,
                    TargetSpeedKnots = null
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error converting FlightAware aircraft data for flight {FlightIdent}", flight.Ident);
            }
        }

        return aircraft;
    }

    private static Vector2 LatLonToNm(double lat, double lon, double centerLat, double centerLon)
    {
        var latDiff = lat - centerLat;
        var lonDiff = lon - centerLon;
        var nmLat = latDiff * 60.0;
        var nmLon = lonDiff * 60.0 * Math.Cos(centerLat * Math.PI / 180.0);
        return new Vector2((float)nmLon, (float)nmLat);
    }
}

public class FlightAwareSearchResponse
{
    [JsonPropertyName("flights")]
    public List<FlightAwareFlight>? Flights { get; set; }
}

public class FlightAwareFlight
{
    [JsonPropertyName("ident")]
    public string? Ident { get; set; }

    [JsonPropertyName("aircraft_type")]
    public string? AircraftType { get; set; }

    [JsonPropertyName("last_position")]
    public FlightAwarePosition? LastPosition { get; set; }
}

public class FlightAwarePosition
{
    [JsonPropertyName("altitude")]
    public int Altitude { get; set; }

    [JsonPropertyName("groundspeed")]
    public int Groundspeed { get; set; }

    [JsonPropertyName("heading")]
    public int? Heading { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
