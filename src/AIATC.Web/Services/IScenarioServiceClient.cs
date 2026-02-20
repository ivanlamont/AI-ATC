using AIATC.ScenarioService.Protos;

namespace AIATC.Web.Services;

/// <summary>
/// Client interface for communicating with the ScenarioService via gRPC
/// </summary>
public interface IScenarioServiceClient
{
    // Scenario Discovery
    Task<ListScenariosResponse> ListScenariosAsync(ListScenariosRequest request);
    Task<ScenarioResponse> GetScenarioAsync(string scenarioId);
    Task<ListScenariosResponse> SearchScenariosAsync(SearchScenariosRequest request);

    // Scenario Lifecycle
    Task<ScenarioSessionResponse> StartScenarioAsync(string scenarioId, string userId);
    Task<SaveProgressResponse> SaveProgressAsync(SaveProgressRequest request);
    Task<SaveProgressResponse> SaveScenarioStateAsync(SaveScenarioStateRequest request);
    Task<ScenarioSessionResponse> LoadProgressAsync(string savedScenarioId);
    Task<ScenarioResultResponse> CompleteScenarioAsync(CompleteScenarioRequest request);

    // Airspace Data
    Task<AirportDataResponse> GetAirportDataAsync(string airportCode);
    Task<ProceduresResponse> GetProceduresAsync(GetProceduresRequest request);
    Task<AirwaysResponse> GetAirwaysAsync(GetAirwaysRequest request);

    // Live Flight Data
    Task<InitialAircraftResponse> GetInitialAircraftPositionsAsync(string airportCode, float radiusNm);
    Task<LiveFlightsResponse> GetLiveFlightsAsync(string airportCode, float radiusNm);

    // Leaderboards
    Task<LeaderboardResponse> GetLeaderboardAsync(string scenarioId, int limit);
    Task<SubmitScoreResponse> SubmitScoreAsync(SubmitScoreRequest request);

    // Health
    Task<HealthCheckResponse> HealthCheckAsync();
}
