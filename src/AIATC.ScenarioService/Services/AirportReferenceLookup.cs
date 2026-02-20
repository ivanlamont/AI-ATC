using AIATC.ReferenceData.Context;
using AIATC.ReferenceData.Models;
using Microsoft.EntityFrameworkCore;

namespace AIATC.ScenarioService.Services;

internal static class AirportReferenceLookup
{
    public static async Task<Airport?> FindAirportAsync(AirspaceReferenceDbContext db, string? code)
    {
        var normalized = Normalize(code);
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        // 1) Direct field matches.
        var direct = await db.Airports.FirstOrDefaultAsync(a =>
            (a.IcaoCode != null && a.IcaoCode.ToUpper() == normalized) ||
            (a.IcaoCode2 != null && a.IcaoCode2.ToUpper() == normalized) ||
            (a.AirportIdentifier != null && a.AirportIdentifier.ToUpper() == normalized) ||
            (a.AtaIataDesignator != null && a.AtaIataDesignator.ToUpper() == normalized));

        if (direct != null)
        {
            return direct;
        }

        // 2) ICAO-style code (e.g. KSFO) -> prefix + local identifier (SFO).
        if (normalized.Length == 4)
        {
            var prefix = normalized[..1];
            var localId = normalized[1..];

            var candidates = await db.Airports
                .Where(a =>
                    (a.AirportIdentifier != null && a.AirportIdentifier.ToUpper() == localId) ||
                    (a.AtaIataDesignator != null && a.AtaIataDesignator.ToUpper() == localId))
                .ToListAsync();

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            return candidates.FirstOrDefault(a =>
                Normalize(a.IcaoCode) == prefix ||
                Normalize(a.IcaoCode2) == prefix);
        }

        return null;
    }

    public static List<string> BuildRunwayLookupCodes(Airport? airport, string? requestedCode)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddRunwayCodeIfPresent(codes, requestedCode);

        var normalizedRequest = Normalize(requestedCode);
        if (normalizedRequest.Length == 4)
        {
            AddRunwayCodeIfPresent(codes, normalizedRequest[1..]);
        }

        AddRunwayCodeIfPresent(codes, airport?.AirportIdentifier);
        AddRunwayCodeIfPresent(codes, airport?.AtaIataDesignator);
        AddRunwayCodeIfPresent(codes, airport?.IcaoCode);
        AddRunwayCodeIfPresent(codes, airport?.IcaoCode2);

        return codes.ToList();
    }

    public static string BuildDisplayAirportCode(Airport? airport, string? requestedCode)
    {
        var requested = Normalize(requestedCode);
        if (requested.Length == 4)
        {
            return requested;
        }

        var local = Normalize(airport?.AirportIdentifier);
        if (local.Length == 4)
        {
            return local;
        }

        if (local.Length == 3)
        {
            var prefix = Normalize(airport?.IcaoCode);
            if (prefix.Length == 1)
            {
                return prefix + local;
            }

            var prefix2 = Normalize(airport?.IcaoCode2);
            if (prefix2.Length == 1)
            {
                return prefix2 + local;
            }
        }

        if (!string.IsNullOrEmpty(local))
        {
            return local;
        }

        var iata = Normalize(airport?.AtaIataDesignator);
        if (!string.IsNullOrEmpty(iata))
        {
            return iata;
        }

        return requested;
    }

    public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static void AddIfPresent(HashSet<string> target, string? value)
    {
        var normalized = Normalize(value);
        if (!string.IsNullOrEmpty(normalized))
        {
            target.Add(normalized);
        }
    }

    private static void AddRunwayCodeIfPresent(HashSet<string> target, string? value)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrEmpty(normalized))
        {
            return;
        }

        // Runway rows should match airport identifiers/codes (e.g. KOAK/OAK), not regional prefixes (e.g. K2).
        // Excluding short codes prevents accidental cross-airport runway joins.
        if (normalized.Length < 3)
        {
            return;
        }

        target.Add(normalized);
    }
}
