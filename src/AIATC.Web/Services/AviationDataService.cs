using System.Text.Json;
using AIATC.Domain.Models.Aviation;
using Serilog;

namespace AIATC.Web.Services;

/// <summary>
/// Service for accessing aviation data from WorldDataService API
/// </summary>
public interface IAviationDataService
{
    /// <summary>
    /// Get all aircraft types from the API
    /// </summary>
    Task<List<AircraftType>> GetAllAircraftTypesAsync();

    /// <summary>
    /// Get aircraft type by ICAO code
    /// </summary>
    Task<AircraftType?> GetAircraftTypeAsync(string icaoCode);

    /// <summary>
    /// Get aircraft types by manufacturer
    /// </summary>
    Task<List<AircraftType>> GetAircraftTypesByManufacturerAsync(string manufacturer);
}

/// <summary>
/// Implementation of aviation data service accessing WorldDataService REST API
/// </summary>
public class AviationDataService : IAviationDataService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AviationDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<AircraftType>> GetAllAircraftTypesAsync()
    {
        try
        {
            Log.Information("Fetching all aircraft types from WorldDataService");
            var response = await _httpClient.GetAsync("api/AircraftTypes");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var aircraftTypes = JsonSerializer.Deserialize<List<AircraftType>>(json, _jsonOptions);
                Log.Information("Successfully retrieved {Count} aircraft types", aircraftTypes?.Count ?? 0);
                return aircraftTypes ?? new List<AircraftType>();
            }
            else
            {
                Log.Warning("Failed to fetch aircraft types. Status: {StatusCode}", response.StatusCode);
                return new List<AircraftType>();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching aircraft types from WorldDataService");
            return new List<AircraftType>();
        }
    }

    public async Task<AircraftType?> GetAircraftTypeAsync(string icaoCode)
    {
        try
        {
            Log.Information("Fetching aircraft type {IcaoCode} from WorldDataService", icaoCode);
            var response = await _httpClient.GetAsync($"api/AircraftTypes/{icaoCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var aircraftType = JsonSerializer.Deserialize<AircraftType>(json, _jsonOptions);
                Log.Information("Successfully retrieved aircraft type {IcaoCode}", icaoCode);
                return aircraftType;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Log.Information("Aircraft type {IcaoCode} not found", icaoCode);
                return null;
            }
            else
            {
                Log.Warning("Failed to fetch aircraft type {IcaoCode}. Status: {StatusCode}", icaoCode, response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching aircraft type {IcaoCode} from WorldDataService", icaoCode);
            return null;
        }
    }

    public async Task<List<AircraftType>> GetAircraftTypesByManufacturerAsync(string manufacturer)
    {
        try
        {
            Log.Information("Fetching aircraft types for manufacturer {Manufacturer} from WorldDataService", manufacturer);
            var response = await _httpClient.GetAsync($"api/AircraftTypes/manufacturer/{Uri.EscapeDataString(manufacturer)}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var aircraftTypes = JsonSerializer.Deserialize<List<AircraftType>>(json, _jsonOptions);
                Log.Information("Successfully retrieved {Count} aircraft types for manufacturer {Manufacturer}", 
                    aircraftTypes?.Count ?? 0, manufacturer);
                return aircraftTypes ?? new List<AircraftType>();
            }
            else
            {
                Log.Warning("Failed to fetch aircraft types for manufacturer {Manufacturer}. Status: {StatusCode}", 
                    manufacturer, response.StatusCode);
                return new List<AircraftType>();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching aircraft types for manufacturer {Manufacturer} from WorldDataService", manufacturer);
            return new List<AircraftType>();
        }
    }
}
