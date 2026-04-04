using System.Text.Json;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Aviation;
using AIATC.Domain.Services;
using Serilog;

namespace AIATC.Web.Services;

/// <summary>
/// Service for accessing real-time flight data via the BFF proxy.
/// The FlightAware API key is held server-side in AIATC.BFF and is never
/// sent to the browser.
/// </summary>
public interface IFlightAwareService
{
    Task<List<AircraftModel>> GetLiveFlightsAsync(double centerLat, double centerLon, float radiusNm);
    Task<List<AircraftModel>> GetLiveFlightsForAirportAsync(string airportIcaoCode, float radiusNm = 50.0f);
}

/// <summary>
/// Calls the BFF proxy endpoints (/api/flights/…) so no API key is
/// ever present in browser network traffic.
/// </summary>
public class FlightAwareService : IFlightAwareService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlightAwareService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<AircraftModel>> GetLiveFlightsAsync(
        double centerLat, double centerLon, float radiusNm)
    {
        try
        {
            var minLat = centerLat - (radiusNm / 60.0);
            var maxLat = centerLat + (radiusNm / 60.0);
            var minLon = centerLon - (radiusNm / (60.0 * Math.Cos(centerLat * Math.PI / 180.0)));
            var maxLon = centerLon + (radiusNm / (60.0 * Math.Cos(centerLat * Math.PI / 180.0)));
            var ceiling = 20000;

            var query = $"-belowAltitude{ceiling}-latlong \"{minLat} {minLon} {maxLat} {maxLon}\"";

            Log.Information("Fetching live flights via BFF proxy for area centered at {Lat},{Lon} r={R}NM",
                centerLat, centerLon, radiusNm);

            // Relative URL — goes to the BFF on the same origin; no x-apikey in browser
            var response = await _httpClient.GetAsync(
                $"/api/flights/search?query={Uri.EscapeDataString(query)}");

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("BFF flights/search returned {Status}", response.StatusCode);
                return new List<AircraftModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var flightData = JsonSerializer.Deserialize<FlightAwareResponse>(json, _jsonOptions);

            if (flightData?.InFlightInfoResult?.Aircraft != null)
            {
                var aircraft = ConvertToAircraftModels(
                    flightData.InFlightInfoResult.Aircraft, centerLat, centerLon);
                Log.Information("Retrieved {Count} live flights", aircraft.Count);
                return aircraft;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching live flights");
        }

        return new List<AircraftModel>();
    }

    public async Task<List<AircraftModel>> GetLiveFlightsForAirportAsync(
        string airportIcaoCode, float radiusNm)
    {
        try
        {
            Log.Information("Fetching live flights for airport {Code} via BFF proxy", airportIcaoCode);

            // Relative URL — BFF proxies to FlightAware with the API key server-side
            var response = await _httpClient.GetAsync(
                $"/api/flights/airport/{Uri.EscapeDataString(airportIcaoCode)}");

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("BFF flights/airport returned {Status}", response.StatusCode);
                return new List<AircraftModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var flightData = JsonSerializer.Deserialize<AirportBoardsResponse>(json, _jsonOptions);

            if (flightData?.AirportBoardsResult?.Enroute != null)
            {
                var aircraft = ConvertToAircraftModels(
                    flightData.AirportBoardsResult.Enroute, airportIcaoCode);
                Log.Information("Retrieved {Count} live flights for {Code}", aircraft.Count, airportIcaoCode);
                return aircraft;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching live flights for airport {Code}", airportIcaoCode);
        }

        return new List<AircraftModel>();
    }

    private List<AircraftModel> ConvertToAircraftModels(
        List<FlightAwareAircraft> faAircraft, double centerLat, double centerLon)
    {
        var result = new List<AircraftModel>();
        foreach (var fa in faAircraft)
        {
            try
            {
                var positionNm = LatLonToNm(fa.Latitude ?? 0, fa.Longitude ?? 0, centerLat, centerLon);
                var callsign = fa.Ident ?? $"FLIGHT{fa.FlightId}";
                result.Add(new AircraftModel
                {
                    Callsign = callsign,
                    PositionNm = positionNm,
                    HeadingDegrees = fa.Heading ?? 0,
                    SpeedKnots = fa.Groundspeed ?? 0,
                    AltitudeFt = fa.Altitude ?? 0,
                    MinSpeedKnots = 160,
                    MaxSpeedKnots = 500,
                    IsArrival = fa.IsArrival ?? false,
                    TargetHeadingDegrees = null,
                    TargetAltitudeFt = null,
                    TargetSpeedKnots = null,
                    AssignedPiperVoice = AirlineVoiceMapper.AssignVoice(callsign).PiperVoiceName
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error converting FlightAware aircraft {FlightId}", fa.FlightId);
            }
        }
        return result;
    }

    private List<AircraftModel> ConvertToAircraftModels(
        List<AirportFlight> flights, string airportIcaoCode)
    {
        var result = new List<AircraftModel>();
        foreach (var flight in flights)
        {
            try
            {
                var rng = new Random(flight.FlightId.GetHashCode());
                var angle = rng.NextDouble() * 2 * Math.PI;
                var distance = rng.NextDouble() * 20.0;

                var callsign = flight.Ident ?? $"FLIGHT{flight.FlightId}";
                result.Add(new AircraftModel
                {
                    Callsign = callsign,
                    PositionNm = new Vector2(
                        (float)(distance * Math.Cos(angle)),
                        (float)(distance * Math.Sin(angle))),
                    HeadingDegrees = (float)(rng.NextDouble() * 360.0),
                    SpeedKnots = rng.Next(180, 450),
                    AltitudeFt = rng.Next(1000, 45000),
                    MinSpeedKnots = 160,
                    MaxSpeedKnots = 500,
                    IsArrival = flight.IsArrival ?? false,
                    TargetHeadingDegrees = null,
                    TargetAltitudeFt = null,
                    TargetSpeedKnots = null,
                    AssignedPiperVoice = AirlineVoiceMapper.AssignVoice(callsign).PiperVoiceName
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error converting airport flight {FlightId}", flight.FlightId);
            }
        }
        return result;
    }

    private static Vector2 LatLonToNm(double lat, double lon, double centerLat, double centerLon)
    {
        var nmLat = (lat - centerLat) * 60.0;
        var nmLon = (lon - centerLon) * 60.0 * Math.Cos(centerLat * Math.PI / 180.0);
        return new Vector2((float)nmLon, (float)nmLat);
    }
}

// FlightAware API response models
public class FlightAwareResponse
{
    public InFlightInfoResult? InFlightInfoResult { get; set; }
}

public class InFlightInfoResult
{
    public List<FlightAwareAircraft>? Aircraft { get; set; }
}

public class FlightAwareAircraft
{
    public int? FlightId { get; set; }
    public string? Ident { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public float? Heading { get; set; }
    public int? Groundspeed { get; set; }
    public int? Altitude { get; set; }
    public bool? IsArrival { get; set; }
}

public class AirportBoardsResponse
{
    public AirportBoardsResult? AirportBoardsResult { get; set; }
}

public class AirportBoardsResult
{
    public List<AirportFlight>? Enroute { get; set; }
}

public class AirportFlight
{
    public int? FlightId { get; set; }
    public string? Ident { get; set; }
    public bool? IsArrival { get; set; }
}
