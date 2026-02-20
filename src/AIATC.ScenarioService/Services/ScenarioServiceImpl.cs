using AIATC.ReferenceData.Context;
using AIATC.ScenarioService.Data;
using AIATC.ScenarioService.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AIATC.ScenarioService.Services;

/// <summary>
/// gRPC service implementation for scenario management and airspace data
/// </summary>
public class ScenarioServiceImpl : Protos.ScenarioService.ScenarioServiceBase
{
    private readonly AirspaceReferenceDbContext _airspaceDb;
    private readonly ScenarioUsageDbContext _usageDb;
    private readonly IFlightAwareService _flightAwareService;

    public ScenarioServiceImpl(
        AirspaceReferenceDbContext airspaceDb,
        ScenarioUsageDbContext usageDb,
        IFlightAwareService flightAwareService)
    {
        _airspaceDb = airspaceDb;
        _usageDb = usageDb;
        _flightAwareService = flightAwareService;
    }

    // Health Check
    public override Task<HealthCheckResponse> HealthCheck(HealthCheckRequest request, ServerCallContext context)
    {
        Log.Information("Health check requested");
        return Task.FromResult(new HealthCheckResponse
        {
            Status = "healthy",
            Version = "1.0.0"
        });
    }

    // Scenario Discovery
    public override async Task<ListScenariosResponse> ListScenarios(ListScenariosRequest request, ServerCallContext context)
    {
        try
        {
            Log.Information("Listing scenarios - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            var query = _usageDb.Scenarios.Where(s => s.IsActive);

            if (!string.IsNullOrEmpty(request.AirportCode))
            {
                query = query.Where(s => s.AirportCode == request.AirportCode);
            }

            if (!string.IsNullOrEmpty(request.Difficulty))
            {
                query = query.Where(s => s.Difficulty == request.Difficulty);
            }

            var totalCount = await query.CountAsync();
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var scenarios = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new ListScenariosResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            foreach (var scenario in scenarios)
            {
                response.Scenarios.Add(MapScenarioToProto(scenario));
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error listing scenarios");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to list scenarios"));
        }
    }

    public override async Task<ScenarioResponse> GetScenario(GetScenarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ScenarioId, out var scenarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid scenario ID"));
            }

            var scenario = await _usageDb.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId);
            if (scenario == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Scenario not found"));
            }

            return MapScenarioToProto(scenario);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting scenario {ScenarioId}", request.ScenarioId);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get scenario"));
        }
    }

    public override async Task<ListScenariosResponse> SearchScenarios(SearchScenariosRequest request, ServerCallContext context)
    {
        try
        {
            var query = _usageDb.Scenarios
                .Where(s => s.IsActive &&
                       (s.Name.Contains(request.Query) ||
                        s.Description!.Contains(request.Query)));

            var totalCount = await query.CountAsync();
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var scenarios = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new ListScenariosResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            foreach (var scenario in scenarios)
            {
                response.Scenarios.Add(MapScenarioToProto(scenario));
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error searching scenarios");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to search scenarios"));
        }
    }

    // Scenario Lifecycle
    public override async Task<ScenarioSessionResponse> StartScenario(StartScenarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ScenarioId, out var scenarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid scenario ID"));
            }

            // Get scenario from usage database
            var scenario = await _usageDb.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId);
            if (scenario == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Scenario not found"));
            }

            var user = await GetOrCreateUserAsync(request.UserId);

            // Get airport data from reference database
            var airport = await AirportReferenceLookup.FindAirportAsync(_airspaceDb, scenario.AirportCode);
            var airportLat = ParseLatitude(airport?.Latitude);
            var airportLon = ParseLongitude(airport?.Longitude);

            // Get initial aircraft positions from FlightAware
            var liveAircraft = await _flightAwareService.GetLiveFlightsForAirportAsync(scenario.AirportCode, 50.0f);

            // Create session
            var session = new Data.Models.Session
            {
                Id = Guid.NewGuid(),
                ScenarioId = scenarioId,
                UserId = user.Id,
                StartedAt = DateTime.UtcNow,
                Status = "active"
            };

            _usageDb.Sessions.Add(session);
            await _usageDb.SaveChangesAsync();

            var response = new ScenarioSessionResponse
            {
                SessionId = session.Id.ToString(),
                ScenarioId = request.ScenarioId
            };

            // Add airport data if found
            if (airport != null)
            {
                var lat = ParseLatitude(airport.Latitude);
                var lon = ParseLongitude(airport.Longitude);
                var elev = ParseInt(airport.Elevation);

                response.AirportData = new AirportDataResponse
                {
                    IcaoCode = AirportReferenceLookup.BuildDisplayAirportCode(airport, scenario.AirportCode),
                    Name = airport.AirportName ?? string.Empty,
                    Latitude = lat,
                    Longitude = lon,
                    ElevationFt = elev
                };
            }

            // Add initial aircraft
            foreach (var aircraft in liveAircraft)
            {
                response.InitialAircraft.Add(MapAircraftToProto(aircraft, airportLat, airportLon));
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting scenario");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to start scenario"));
        }
    }

    public override async Task<SaveProgressResponse> SaveProgress(SaveProgressRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.SessionId, out var sessionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID"));
            }

            var session = await _usageDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found"));
            }

            var savedScenario = new Data.Models.SavedScenario
            {
                Id = Guid.NewGuid(),
                ScenarioId = session.ScenarioId,
                UserId = session.UserId,
                Name = request.SaveName,
                SavedState = request.SavedState,
                SavedAt = DateTime.UtcNow,
                ProgressPercentage = (decimal)request.ProgressPercentage
            };

            _usageDb.SavedScenarios.Add(savedScenario);
            await _usageDb.SaveChangesAsync();

            return new SaveProgressResponse
            {
                SavedScenarioId = savedScenario.Id.ToString(),
                Success = true,
                Message = "Progress saved successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving progress");
            return new SaveProgressResponse
            {
                Success = false,
                Message = "Failed to save progress"
            };
        }
    }

    public override async Task<SaveProgressResponse> SaveScenarioState(SaveScenarioStateRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "User ID is required"));
            }

            if (string.IsNullOrWhiteSpace(request.AirportCode))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Airport code is required"));
            }

            var user = await GetOrCreateUserAsync(request.UserId);
            var scenario = await ResolveScenarioForSaveAsync(request);

            var saveName = string.IsNullOrWhiteSpace(request.SaveName)
                ? $"Saved {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                : request.SaveName.Trim();

            var savedScenario = new Data.Models.SavedScenario
            {
                Id = Guid.NewGuid(),
                ScenarioId = scenario.Id,
                UserId = user.Id,
                Name = saveName,
                SavedState = request.SavedState ?? string.Empty,
                SavedAt = DateTime.UtcNow,
                ProgressPercentage = (decimal)request.ProgressPercentage
            };

            _usageDb.SavedScenarios.Add(savedScenario);
            await _usageDb.SaveChangesAsync();

            return new SaveProgressResponse
            {
                SavedScenarioId = savedScenario.Id.ToString(),
                Success = true,
                Message = "Scenario saved successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving scenario state");
            return new SaveProgressResponse
            {
                Success = false,
                Message = "Failed to save scenario state"
            };
        }
    }

    public override async Task<ScenarioSessionResponse> LoadProgress(LoadProgressRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.SavedScenarioId, out var savedScenarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid saved scenario ID"));
            }

            var savedScenario = await _usageDb.SavedScenarios
                .Include(ss => ss.Scenario)
                .FirstOrDefaultAsync(ss => ss.Id == savedScenarioId);

            if (savedScenario == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Saved scenario not found"));
            }

            return new ScenarioSessionResponse
            {
                SessionId = Guid.NewGuid().ToString(),
                ScenarioId = savedScenario.ScenarioId.ToString(),
                InitialState = savedScenario.SavedState
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading progress");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to load progress"));
        }
    }

    public override async Task<ScenarioResultResponse> CompleteScenario(CompleteScenarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.SessionId, out var sessionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID"));
            }

            var session = await _usageDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found"));
            }

            session.Status = "completed";
            session.EndedAt = DateTime.UtcNow;
            session.Score = request.Score;
            session.Metrics = request.Metrics;

            // Create score entry
            var score = new Data.Models.Score
            {
                Id = Guid.NewGuid(),
                ScenarioId = session.ScenarioId,
                UserId = session.UserId,
                ScoreValue = request.Score,
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds
            };

            _usageDb.Scores.Add(score);
            await _usageDb.SaveChangesAsync();

            // Calculate rank
            var rank = await _usageDb.Scores
                .Where(s => s.ScenarioId == session.ScenarioId && s.ScoreValue > request.Score)
                .CountAsync() + 1;

            return new ScenarioResultResponse
            {
                Success = true,
                Score = request.Score,
                Rank = rank,
                Message = "Scenario completed successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error completing scenario");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to complete scenario"));
        }
    }

    // Airspace Data
    public override async Task<AirportDataResponse> GetAirportData(GetAirportRequest request, ServerCallContext context)
    {
        try
        {
            var airport = await AirportReferenceLookup.FindAirportAsync(_airspaceDb, request.AirportCode);
            if (airport == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Airport not found"));
            }

            var lat = ParseLatitude(airport.Latitude);
            var lon = ParseLongitude(airport.Longitude);
            var elev = ParseInt(airport.Elevation);

            var response = new AirportDataResponse
            {
                IcaoCode = AirportReferenceLookup.BuildDisplayAirportCode(airport, request.AirportCode),
                Name = airport.AirportName ?? string.Empty,
                Latitude = lat,
                Longitude = lon,
                ElevationFt = elev
            };

            var runwayCodes = AirportReferenceLookup.BuildRunwayLookupCodes(airport, request.AirportCode);
            var runways = await _airspaceDb.Runways
                .Where(r =>
                    (r.IcaoCode != null && runwayCodes.Contains(r.IcaoCode.ToUpper())) ||
                    (r.AirportIdentifier != null && runwayCodes.Contains(r.AirportIdentifier.ToUpper())))
                .ToListAsync();

            var runwayRecords = runways
                .Select(r => new RunwayRecord
                {
                    Identifier = r.RunwayIdentifier?.Trim() ?? string.Empty,
                    LengthFt = ParseDouble(r.RunwayLength),
                    WidthFt = ParseDouble(r.Width),
                    Heading = ParseRunwayBearing(r.RunwayBearing),
                    Latitude = ParseLatitude(r.Latitude, lat),
                    Longitude = ParseLongitude(r.Longitude, lon)
                })
                .ToList();

            // Runway lookup can still return duplicate identifiers from nearby code collisions.
            // Keep the closest threshold per designator to the resolved airport, then merge reciprocals.
            var deduplicatedRunwayRecords = runwayRecords
                .GroupBy(r => NormalizeRunwayDesignator(r.Identifier) ?? r.Identifier.Trim().ToUpperInvariant())
                .Select(g => g
                    .OrderBy(r => CalculateDistanceNm(lat, lon, r.Latitude, r.Longitude))
                    .First())
                .ToList();

            var groupedRunways = deduplicatedRunwayRecords
                .GroupBy(r => BuildPhysicalRunwayKey(r.Identifier))
                .ToList();

            foreach (var group in groupedRunways)
            {
                var merged = MergeRunwayGroup(group.ToList());
                response.Runways.Add(new RunwayData
                {
                    Identifier = merged.Identifier,
                    LengthFt = merged.LengthFt,
                    WidthFt = merged.WidthFt,
                    Heading = merged.Heading,
                    Latitude = merged.Latitude,
                    Longitude = merged.Longitude
                });
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting airport data");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get airport data"));
        }
    }

    private static RunwayRecord MergeRunwayGroup(List<RunwayRecord> group)
    {
        if (group.Count == 0)
        {
            return new RunwayRecord();
        }

        if (group.Count == 1)
        {
            var single = group[0];
            return new RunwayRecord
            {
                Identifier = single.Identifier,
                LengthFt = single.LengthFt,
                WidthFt = single.WidthFt,
                Heading = NormalizeHeadingWithDesignatorFallback(single.Identifier, single.Heading),
                Latitude = single.Latitude,
                Longitude = single.Longitude
            };
        }

        // Prefer first reciprocal pair for geometric merge.
        var primary = group[0];
        var reciprocal = FindReciprocal(primary, group);
        if (reciprocal == null)
        {
            return new RunwayRecord
            {
                Identifier = primary.Identifier,
                LengthFt = primary.LengthFt,
                WidthFt = group.Average(r => r.WidthFt),
                Heading = NormalizeHeadingWithDesignatorFallback(primary.Identifier, primary.Heading),
                Latitude = primary.Latitude,
                Longitude = primary.Longitude
            };
        }

        var computedHeading = CalculateBearingDegrees(
            primary.Latitude, primary.Longitude,
            reciprocal.Latitude, reciprocal.Longitude);
        var computedLengthNm = CalculateDistanceNm(
            primary.Latitude, primary.Longitude,
            reciprocal.Latitude, reciprocal.Longitude);
        var computedLengthFt = computedLengthNm * 6076.12;
        var isReasonableReciprocalDistance = computedLengthNm >= 0.2 && computedLengthNm <= 10.0;

        var width = group.Average(r => r.WidthFt);
        var idA = primary.Identifier.Trim();
        var idB = reciprocal.Identifier.Trim();
        var mergedIdentifier = string.IsNullOrWhiteSpace(idA) || string.IsNullOrWhiteSpace(idB)
            ? (idA + idB)
            : $"{idA}/{idB}";

        return new RunwayRecord
        {
            Identifier = mergedIdentifier,
            LengthFt = isReasonableReciprocalDistance && computedLengthFt > 0 ? computedLengthFt : group.Max(r => r.LengthFt),
            WidthFt = width > 0 ? width : group.Max(r => r.WidthFt),
            Heading = NormalizeHeadingWithDesignatorFallback(primary.Identifier,
                isReasonableReciprocalDistance ? computedHeading : primary.Heading),
            Latitude = primary.Latitude,
            Longitude = primary.Longitude
        };
    }

    private static RunwayRecord? FindReciprocal(RunwayRecord source, List<RunwayRecord> candidates)
    {
        var expected = BuildReciprocalRunwayDesignator(source.Identifier);
        if (expected == null)
        {
            return null;
        }

        return candidates
            .Where(r =>
            !ReferenceEquals(r, source) &&
            NormalizeRunwayDesignator(r.Identifier) == expected)
            .OrderBy(r => CalculateDistanceNm(source.Latitude, source.Longitude, r.Latitude, r.Longitude))
            .FirstOrDefault();
    }

    private static string BuildPhysicalRunwayKey(string? runwayIdentifier)
    {
        if (!string.IsNullOrWhiteSpace(runwayIdentifier) && runwayIdentifier.Contains('/'))
        {
            var parts = runwayIdentifier.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                var a = NormalizeRunwayDesignator(parts[0]);
                var b = NormalizeRunwayDesignator(parts[1]);
                if (a != null && b != null)
                {
                    return string.CompareOrdinal(a, b) <= 0
                        ? $"{a}|{b}"
                        : $"{b}|{a}";
                }
            }
        }

        var normalized = NormalizeRunwayDesignator(runwayIdentifier);
        if (normalized == null)
        {
            return (runwayIdentifier ?? string.Empty).Trim().ToUpperInvariant();
        }

        var reciprocal = BuildReciprocalRunwayDesignator(normalized);
        if (reciprocal == null)
        {
            return normalized;
        }

        return string.CompareOrdinal(normalized, reciprocal) <= 0
            ? $"{normalized}|{reciprocal}"
            : $"{reciprocal}|{normalized}";
    }

    private static string? BuildReciprocalRunwayDesignator(string normalized)
    {
        var normalizedToken = NormalizeRunwayDesignator(normalized);
        if (normalizedToken == null)
        {
            return null;
        }

        if (!int.TryParse(normalizedToken[..2], out var runwayNumber))
        {
            return null;
        }

        if (runwayNumber is < 1 or > 36)
        {
            return null;
        }

        var side = normalizedToken.Length > 2 ? normalizedToken[2].ToString() : string.Empty;
        var reciprocalNumber = ((runwayNumber + 18 - 1) % 36) + 1;
        var reciprocalSide = side switch
        {
            "L" => "R",
            "R" => "L",
            _ => side
        };

        return $"{reciprocalNumber:00}{reciprocalSide}";
    }

    private static string? NormalizeRunwayDesignator(string? runwayIdentifier)
    {
        if (string.IsNullOrWhiteSpace(runwayIdentifier))
        {
            return null;
        }

        var token = runwayIdentifier.Trim().ToUpperInvariant();
        if (token.StartsWith("RW", StringComparison.Ordinal))
        {
            token = token[2..];
        }

        if (token.Length < 1 || token.Length > 3)
        {
            return null;
        }

        var side = string.Empty;
        var numberToken = token;

        var last = token[^1];
        if (char.IsLetter(last))
        {
            if (last is not ('L' or 'R' or 'C'))
            {
                return null;
            }

            side = last.ToString();
            numberToken = token[..^1];
        }

        if (!int.TryParse(numberToken, out var runwayNumber) || runwayNumber is < 1 or > 36)
        {
            return null;
        }

        return $"{runwayNumber:00}{side}";
    }

    private static double NormalizeHeadingWithDesignatorFallback(string? runwayIdentifier, double heading)
    {
        if (heading >= 0 && heading <= 360)
        {
            return heading;
        }

        var normalized = NormalizeRunwayDesignator(runwayIdentifier);
        if (normalized == null || !int.TryParse(normalized[..2], out var runwayNumber))
        {
            return heading;
        }

        var fallback = runwayNumber * 10.0;
        return fallback >= 360 ? 0 : fallback;
    }

    private static double CalculateDistanceNm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        const double kmToNauticalMiles = 0.539957;
        const double degToRad = Math.PI / 180.0;

        var dLat = (lat2 - lat1) * degToRad;
        var dLon = (lon2 - lon1) * degToRad;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * degToRad) * Math.Cos(lat2 * degToRad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c * kmToNauticalMiles;
    }

    private static double CalculateBearingDegrees(double lat1, double lon1, double lat2, double lon2)
    {
        const double degToRad = Math.PI / 180.0;
        const double radToDeg = 180.0 / Math.PI;

        var dLon = (lon2 - lon1) * degToRad;
        var lat1Rad = lat1 * degToRad;
        var lat2Rad = lat2 * degToRad;
        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);
        var brng = Math.Atan2(y, x) * radToDeg;
        return (brng + 360) % 360;
    }

    private sealed class RunwayRecord
    {
        public string Identifier { get; set; } = string.Empty;
        public double LengthFt { get; set; }
        public double WidthFt { get; set; }
        public double Heading { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public override async Task<ProceduresResponse> GetProcedures(GetProceduresRequest request, ServerCallContext context)
    {
        try
        {
            var response = new ProceduresResponse();
            // Implementation would query approach procedures from reference database
            // This is a simplified version
            Log.Warning("GetProcedures not fully implemented yet");
            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting procedures");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get procedures"));
        }
    }

    public override async Task<AirwaysResponse> GetAirways(GetAirwaysRequest request, ServerCallContext context)
    {
        try
        {
            var response = new AirwaysResponse();
            // Implementation would query airways from reference database
            Log.Warning("GetAirways not fully implemented yet");
            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting airways");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get airways"));
        }
    }

    // Live Flight Data
    public override async Task<InitialAircraftResponse> GetInitialAircraftPositions(GetInitialAircraftRequest request, ServerCallContext context)
    {
        try
        {
            var aircraft = await _flightAwareService.GetLiveFlightsForAirportAsync(request.AirportCode, request.RadiusNm);
            var airport = await AirportReferenceLookup.FindAirportAsync(_airspaceDb, request.AirportCode);
            var airportLat = ParseLatitude(airport?.Latitude);
            var airportLon = ParseLongitude(airport?.Longitude);

            var response = new InitialAircraftResponse();
            foreach (var ac in aircraft)
            {
                response.Aircraft.Add(MapAircraftToProto(ac, airportLat, airportLon));
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting initial aircraft positions");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get aircraft positions"));
        }
    }

    public override async Task<LiveFlightsResponse> GetLiveFlights(GetLiveFlightsRequest request, ServerCallContext context)
    {
        try
        {
            var aircraft = await _flightAwareService.GetLiveFlightsForAirportAsync(request.AirportCode, request.RadiusNm);
            var airport = await AirportReferenceLookup.FindAirportAsync(_airspaceDb, request.AirportCode);
            var airportLat = ParseLatitude(airport?.Latitude);
            var airportLon = ParseLongitude(airport?.Longitude);

            var response = new LiveFlightsResponse();
            foreach (var ac in aircraft)
            {
                response.Aircraft.Add(MapAircraftToProto(ac, airportLat, airportLon));
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting live flights");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get live flights"));
        }
    }

    // Leaderboards
    public override async Task<LeaderboardResponse> GetLeaderboard(GetLeaderboardRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ScenarioId, out var scenarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid scenario ID"));
            }

            var limit = request.Limit > 0 ? request.Limit : 10;

            var topScores = await _usageDb.Scores
                .Include(s => s.User)
                .Where(s => s.ScenarioId == scenarioId)
                .OrderByDescending(s => s.ScoreValue)
                .ThenBy(s => s.DurationSeconds)
                .Take(limit)
                .ToListAsync();

            var response = new LeaderboardResponse();
            int rank = 1;
            foreach (var score in topScores)
            {
                response.Entries.Add(new LeaderboardEntry
                {
                    Rank = rank++,
                    UserName = score.User.DisplayName ?? score.User.Username,
                    Score = score.ScoreValue,
                    DurationSeconds = score.DurationSeconds ?? 0,
                    CompletedAt = score.CompletedAt.ToString("o")
                });
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting leaderboard");
            throw new RpcException(new Status(StatusCode.Internal, "Failed to get leaderboard"));
        }
    }

    public override async Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.SessionId, out var sessionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID"));
            }

            var user = await GetOrCreateUserAsync(request.UserId);

            var session = await _usageDb.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found"));
            }

            var score = new Data.Models.Score
            {
                Id = Guid.NewGuid(),
                ScenarioId = session.ScenarioId,
                UserId = user.Id,
                ScoreValue = request.Score,
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = request.DurationSeconds
            };

            _usageDb.Scores.Add(score);
            await _usageDb.SaveChangesAsync();

            // Calculate rank
            var rank = await _usageDb.Scores
                .Where(s => s.ScenarioId == session.ScenarioId && s.ScoreValue > request.Score)
                .CountAsync() + 1;

            return new SubmitScoreResponse
            {
                Success = true,
                Rank = rank,
                Message = "Score submitted successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error submitting score");
            return new SubmitScoreResponse
            {
                Success = false,
                Message = "Failed to submit score"
            };
        }
    }

    // Helper methods
    private ScenarioResponse MapScenarioToProto(Data.Models.Scenario scenario)
    {
        return new ScenarioResponse
        {
            Id = scenario.Id.ToString(),
            Name = scenario.Name,
            Description = scenario.Description ?? string.Empty,
            AirportCode = scenario.AirportCode,
            Difficulty = scenario.Difficulty ?? string.Empty,
            DurationMinutes = scenario.DurationMinutes ?? 0,
            Configuration = scenario.Configuration ?? string.Empty,
            Objectives = scenario.Objectives ?? string.Empty
        };
    }

    private AircraftPosition MapAircraftToProto(Domain.Models.AircraftModel aircraft, double referenceLatitude, double referenceLongitude)
    {
        var (latitude, longitude) = NmToLatLon(aircraft.PositionNm, referenceLatitude, referenceLongitude);

        return new AircraftPosition
        {
            Callsign = aircraft.Callsign,
            AircraftType = aircraft.AircraftType ?? string.Empty,
            Latitude = latitude,
            Longitude = longitude,
            AltitudeFt = (int)aircraft.AltitudeFt,
            Heading = aircraft.HeadingDegrees,
            GroundSpeedKnots = aircraft.SpeedKnots,
            VerticalSpeedFpm = aircraft.VerticalSpeedFpm,
            Origin = string.Empty, // Not available in AircraftModel
            Destination = aircraft.Destination?.IcaoCode ?? string.Empty
        };
    }

    private static double ParseDouble(string? value, double defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    private static double ParseRunwayBearing(string? value, double defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        // ARINC runway bearings are often encoded in tenths of degrees, e.g. "0100" => 10.0 deg.
        // Keep decimal forms as-is, otherwise convert 4+ digit integer payloads from tenths.
        var token = value.Trim().ToUpperInvariant();
        var hasDecimal = token.Contains('.');
        var numericToken = new string(token.Where(c => char.IsDigit(c) || c == '.' || c == '-' || c == '+').ToArray());
        if (string.IsNullOrWhiteSpace(numericToken))
        {
            return defaultValue;
        }

        if (!double.TryParse(numericToken, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return defaultValue;
        }

        if (!hasDecimal)
        {
            var digitsOnly = new string(token.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length >= 4)
            {
                parsed /= 10.0;
            }
        }

        return parsed;
    }

    private static int ParseInt(string? value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    private static double ParseLatitude(string? value, double defaultValue = 0) =>
        ArincCoordinateParser.TryParseLatitude(value, out var parsed) ? parsed : defaultValue;

    private static double ParseLongitude(string? value, double defaultValue = 0) =>
        ArincCoordinateParser.TryParseLongitude(value, out var parsed) ? parsed : defaultValue;

    private static (double latitude, double longitude) NmToLatLon(Domain.Models.Vector2 positionNm, double referenceLatitude, double referenceLongitude)
    {
        var latitude = referenceLatitude + (positionNm.Y / 60.0);
        var cosLat = Math.Cos(referenceLatitude * Math.PI / 180.0);

        var longitude = referenceLongitude;
        if (Math.Abs(cosLat) > 1e-6)
        {
            longitude += positionNm.X / (60.0 * cosLat);
        }

        return (latitude, longitude);
    }

    private async Task<Data.Models.User> GetOrCreateUserAsync(string externalUserId)
    {
        var normalized = (externalUserId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "User ID is required"));
        }

        var userId = ToDeterministicGuid(normalized);
        var existing = await _usageDb.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (existing != null)
        {
            existing.LastLoginAt = DateTime.UtcNow;
            await _usageDb.SaveChangesAsync();
            return existing;
        }

        var username = BuildUsername(normalized, userId);
        var user = new Data.Models.User
        {
            Id = userId,
            Username = username,
            DisplayName = username,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _usageDb.Users.Add(user);
        await _usageDb.SaveChangesAsync();
        return user;
    }

    private async Task<Data.Models.Scenario> ResolveScenarioForSaveAsync(SaveScenarioStateRequest request)
    {
        if (Guid.TryParse(request.ScenarioId, out var explicitScenarioId))
        {
            var explicitScenario = await _usageDb.Scenarios.FirstOrDefaultAsync(s => s.Id == explicitScenarioId);
            if (explicitScenario != null)
            {
                return explicitScenario;
            }
        }

        var airportCode = AirportReferenceLookup.Normalize(request.AirportCode);
        var airportCodeLocal = airportCode.Length == 4 ? airportCode[1..] : airportCode;

        var scenario = await _usageDb.Scenarios.FirstOrDefaultAsync(s =>
            s.IsActive && (s.AirportCode == airportCode || s.AirportCode == airportCodeLocal));

        if (scenario != null)
        {
            return scenario;
        }

        var createdScenario = new Data.Models.Scenario
        {
            Id = Guid.NewGuid(),
            Name = $"{airportCode} Custom Save",
            Description = "User-created saved scenario state",
            AirportCode = airportCode.Length > 4 ? airportCode[..4] : airportCode,
            Difficulty = "custom",
            DurationMinutes = 30,
            Configuration = "{}",
            Objectives = "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _usageDb.Scenarios.Add(createdScenario);
        await _usageDb.SaveChangesAsync();
        return createdScenario;
    }

    private static Guid ToDeterministicGuid(string source)
    {
        if (Guid.TryParse(source, out var existing))
        {
            return existing;
        }

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(source));
        return new Guid(hash);
    }

    private static string BuildUsername(string source, Guid userId)
    {
        var sanitized = new string(source.Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.').ToArray());
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "oauth";
        }

        if (sanitized.Length > 85)
        {
            sanitized = sanitized[..85];
        }

        return $"{sanitized}_{userId.ToString("N")[..8]}";
    }
}
