using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Database of navigation fixes, procedures, and airways
/// </summary>
public class NavigationDatabase
{
    private readonly Dictionary<string, Fix> _fixes = new();
    private readonly Dictionary<string, Procedure> _procedures = new();

    /// <summary>
    /// Adds a fix to the database
    /// </summary>
    public void AddFix(Fix fix)
    {
        _fixes[fix.Identifier.ToUpperInvariant()] = fix;
    }

    /// <summary>
    /// Gets a fix by identifier
    /// </summary>
    public Fix? GetFix(string identifier)
    {
        return _fixes.TryGetValue(identifier.ToUpperInvariant(), out var fix) ? fix : null;
    }

    /// <summary>
    /// Gets all fixes within a radius of a position
    /// </summary>
    public List<Fix> GetFixesNear(Vector2 position, float radiusNm)
    {
        return _fixes.Values
            .Where(f => f.GetDistanceNm(position) <= radiusNm)
            .OrderBy(f => f.GetDistanceNm(position))
            .ToList();
    }

    /// <summary>
    /// Adds a procedure to the database
    /// </summary>
    public void AddProcedure(Procedure procedure)
    {
        var key = $"{procedure.AirportIdentifier}_{procedure.Identifier}".ToUpperInvariant();
        _procedures[key] = procedure;
    }

    /// <summary>
    /// Gets a procedure by airport and identifier
    /// </summary>
    public Procedure? GetProcedure(string airportIdentifier, string procedureIdentifier)
    {
        var key = $"{airportIdentifier}_{procedureIdentifier}".ToUpperInvariant();
        return _procedures.TryGetValue(key, out var proc) ? proc : null;
    }

    /// <summary>
    /// Gets all procedures for an airport
    /// </summary>
    public List<Procedure> GetProceduresForAirport(string airportIdentifier)
    {
        return _procedures.Values
            .Where(p => p.AirportIdentifier.Equals(airportIdentifier, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets SIDs for a specific runway
    /// </summary>
    public List<Procedure> GetSidsForRunway(string airportIdentifier, string runwayIdentifier)
    {
        return GetProceduresForAirport(airportIdentifier)
            .Where(p => p.Type == ProcedureType.SID &&
                       (p.RunwayIdentifier == null ||
                        p.RunwayIdentifier.Equals(runwayIdentifier, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Gets STARs for a specific runway
    /// </summary>
    public List<Procedure> GetStarsForRunway(string airportIdentifier, string runwayIdentifier)
    {
        return GetProceduresForAirport(airportIdentifier)
            .Where(p => p.Type == ProcedureType.STAR &&
                       (p.RunwayIdentifier == null ||
                        p.RunwayIdentifier.Equals(runwayIdentifier, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Gets approaches for a specific runway
    /// </summary>
    public List<Procedure> GetApproachesForRunway(string airportIdentifier, string runwayIdentifier)
    {
        return GetProceduresForAirport(airportIdentifier)
            .Where(p => p.Type == ProcedureType.Approach &&
                       p.RunwayIdentifier != null &&
                       p.RunwayIdentifier.Equals(runwayIdentifier, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Builds a direct route between two fixes
    /// </summary>
    public Route? BuildDirectRoute(string fromFixId, string toFixId)
    {
        var fromFix = GetFix(fromFixId);
        var toFix = GetFix(toFixId);

        if (fromFix == null || toFix == null)
            return null;

        var route = new Route();
        route.AddFix(fromFix);
        route.AddFix(toFix);

        return route;
    }

    /// <summary>
    /// Builds a route from current position to a fix
    /// </summary>
    public Route? BuildRouteToFix(Vector2 currentPosition, string fixId)
    {
        var fix = GetFix(fixId);
        if (fix == null)
            return null;

        // Create temporary fix at current position
        var currentFix = new Fix
        {
            Identifier = "CURRENT",
            PositionNm = currentPosition
        };

        var route = new Route();
        route.AddFix(currentFix);
        route.AddFix(fix);

        return route;
    }

    /// <summary>
    /// Gets the count of fixes in the database
    /// </summary>
    public int FixCount => _fixes.Count;

    /// <summary>
    /// Gets the count of procedures in the database
    /// </summary>
    public int ProcedureCount => _procedures.Count;
}
