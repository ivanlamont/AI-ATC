using AIATC.ScenarioService.Protos;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;

namespace AIATC.Web.Services;

/// <summary>
/// gRPC client for communicating with the ScenarioService from Blazor WebAssembly.
/// Uses gRPC-Web for browser compatibility. Calls are forwarded to the BFF (same
/// origin), which proxies them internally to the ScenarioService — no cross-origin
/// connection required and no service URL baked into the WASM build.
/// </summary>
public class ScenarioServiceClient : IScenarioServiceClient
{
    private readonly ScenarioService.Protos.ScenarioService.ScenarioServiceClient _client;

    public ScenarioServiceClient(IWebAssemblyHostEnvironment hostEnvironment)
    {
        // Point at the BFF origin (same host that served the WASM). The BFF uses
        // YARP to proxy /aiatc.scenario.ScenarioService/* to the internal service.
        var address = hostEnvironment.BaseAddress.TrimEnd('/');

        // Create gRPC-Web handler for browser compatibility
        var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

        var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            MaxReceiveMessageSize = 16 * 1024 * 1024,
            MaxSendMessageSize = 16 * 1024 * 1024
        });

        _client = new ScenarioService.Protos.ScenarioService.ScenarioServiceClient(channel);

        Log.Information("ScenarioServiceClient initialized, proxying via BFF at {Address}", address);
    }

    // Scenario Discovery
    public async Task<ListScenariosResponse> ListScenariosAsync(ListScenariosRequest request)
    {
        try
        {
            return await _client.ListScenariosAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error listing scenarios");
            throw;
        }
    }

    public async Task<ScenarioResponse> GetScenarioAsync(string scenarioId)
    {
        try
        {
            var request = new GetScenarioRequest { ScenarioId = scenarioId };
            return await _client.GetScenarioAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting scenario {ScenarioId}", scenarioId);
            throw;
        }
    }

    public async Task<ListScenariosResponse> SearchScenariosAsync(SearchScenariosRequest request)
    {
        try
        {
            return await _client.SearchScenariosAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error searching scenarios");
            throw;
        }
    }

    // Scenario Lifecycle
    public async Task<ScenarioSessionResponse> StartScenarioAsync(string scenarioId, string userId)
    {
        try
        {
            var request = new StartScenarioRequest
            {
                ScenarioId = scenarioId,
                UserId = userId
            };
            return await _client.StartScenarioAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting scenario {ScenarioId}", scenarioId);
            throw;
        }
    }

    public async Task<SaveProgressResponse> SaveProgressAsync(SaveProgressRequest request)
    {
        try
        {
            return await _client.SaveProgressAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving progress");
            throw;
        }
    }

    public async Task<SaveProgressResponse> SaveScenarioStateAsync(SaveScenarioStateRequest request)
    {
        try
        {
            return await _client.SaveScenarioStateAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving scenario state");
            throw;
        }
    }

    public async Task<ScenarioSessionResponse> LoadProgressAsync(string savedScenarioId)
    {
        try
        {
            var request = new LoadProgressRequest { SavedScenarioId = savedScenarioId };
            return await _client.LoadProgressAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading progress");
            throw;
        }
    }

    public async Task<ScenarioResultResponse> CompleteScenarioAsync(CompleteScenarioRequest request)
    {
        try
        {
            return await _client.CompleteScenarioAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error completing scenario");
            throw;
        }
    }

    // Airspace Data
    public async Task<AirportDataResponse> GetAirportDataAsync(string airportCode)
    {
        try
        {
            var request = new GetAirportRequest { AirportCode = airportCode };
            return await _client.GetAirportDataAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting airport data for {AirportCode}", airportCode);
            throw;
        }
    }

    public async Task<ProceduresResponse> GetProceduresAsync(GetProceduresRequest request)
    {
        try
        {
            return await _client.GetProceduresAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting procedures");
            throw;
        }
    }

    public async Task<AirwaysResponse> GetAirwaysAsync(GetAirwaysRequest request)
    {
        try
        {
            return await _client.GetAirwaysAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting airways");
            throw;
        }
    }

    // Live Flight Data
    public async Task<InitialAircraftResponse> GetInitialAircraftPositionsAsync(string airportCode, float radiusNm)
    {
        try
        {
            var request = new GetInitialAircraftRequest
            {
                AirportCode = airportCode,
                RadiusNm = radiusNm
            };
            return await _client.GetInitialAircraftPositionsAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting initial aircraft positions");
            throw;
        }
    }

    public async Task<LiveFlightsResponse> GetLiveFlightsAsync(string airportCode, float radiusNm)
    {
        try
        {
            var request = new GetLiveFlightsRequest
            {
                AirportCode = airportCode,
                RadiusNm = radiusNm
            };
            return await _client.GetLiveFlightsAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting live flights");
            throw;
        }
    }

    // Leaderboards
    public async Task<LeaderboardResponse> GetLeaderboardAsync(string scenarioId, int limit)
    {
        try
        {
            var request = new GetLeaderboardRequest
            {
                ScenarioId = scenarioId,
                Limit = limit
            };
            return await _client.GetLeaderboardAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting leaderboard");
            throw;
        }
    }

    public async Task<SubmitScoreResponse> SubmitScoreAsync(SubmitScoreRequest request)
    {
        try
        {
            return await _client.SubmitScoreAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error submitting score");
            throw;
        }
    }

    // Health
    public async Task<HealthCheckResponse> HealthCheckAsync()
    {
        try
        {
            var request = new HealthCheckRequest();
            return await _client.HealthCheckAsync(request);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking health");
            throw;
        }
    }
}
