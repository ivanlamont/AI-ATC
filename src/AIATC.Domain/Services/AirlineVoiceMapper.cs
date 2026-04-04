using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Services;

/// <summary>
/// Maps airlines to nationality-appropriate Piper TTS voices and assigns
/// a consistent voice to each flight based on its callsign.
/// </summary>
public static class AirlineVoiceMapper
{
    public enum VoiceGender { Male, Female }

    public record VoiceAssignment(string PiperVoiceName, VoiceGender Gender, string Accent);

    private record VoiceOption(string PiperVoiceName, VoiceGender Gender);

    // ── Accent → available voices ───────────────────────────────────────

    private static readonly Dictionary<string, List<VoiceOption>> VoiceCatalog = new()
    {
        ["American"] = new()
        {
            new("en_US-ryan-high",         VoiceGender.Male),
            new("en_US-joe-medium",        VoiceGender.Male),
            new("en_US-bryce-medium",      VoiceGender.Male),
            new("en_US-john-medium",       VoiceGender.Male),
            new("en_US-lessac-high",       VoiceGender.Female),
            new("en_US-amy-medium",        VoiceGender.Female),
            new("en_US-kristin-medium",    VoiceGender.Female),
            new("en_US-hfc_female-medium", VoiceGender.Female),
        },
        ["British"] = new()
        {
            new("en_GB-alan-medium",                  VoiceGender.Male),
            new("en_GB-northern_english_male-medium",  VoiceGender.Male),
            new("en_GB-cori-high",                    VoiceGender.Female),
            new("en_GB-alba-medium",                  VoiceGender.Female),
        },
        ["Irish"] = new()
        {
            // No en_IE voices in Piper — use Northern English as closest proxy
            new("en_GB-northern_english_male-medium", VoiceGender.Male),
            new("en_GB-alba-medium",                  VoiceGender.Female),
        },
        ["French"] = new()
        {
            new("fr_FR-tom-medium",   VoiceGender.Male),
            new("fr_FR-siwis-medium", VoiceGender.Female),
        },
        ["German"] = new()
        {
            new("de_DE-thorsten-high", VoiceGender.Male),
            new("de_DE-kerstin-low",   VoiceGender.Female),
        },
        ["Italian"] = new()
        {
            new("it_IT-riccardo-x_low", VoiceGender.Male),
            new("it_IT-paola-medium",   VoiceGender.Female),
        },
        ["Spanish"] = new()
        {
            new("es_ES-davefx-medium", VoiceGender.Male),
            new("es_MX-claude-high",   VoiceGender.Female),
        },
        ["Dutch"] = new()
        {
            new("nl_NL-pim-medium", VoiceGender.Male),
            new("nl_NL-mls-medium", VoiceGender.Female),
        },
        ["Russian"] = new()
        {
            new("ru_RU-denis-medium",  VoiceGender.Male),
            new("ru_RU-irina-medium",  VoiceGender.Female),
        },
        ["Polish"] = new()
        {
            new("pl_PL-darkman-medium", VoiceGender.Male),
            new("pl_PL-gosia-medium",   VoiceGender.Female),
        },
        ["Scandinavian"] = new()
        {
            new("no_NO-talesyntese-medium", VoiceGender.Male),
            new("sv_SE-lisa-medium",        VoiceGender.Female),
        },
    };

    // ── ICAO airline prefix → accent ────────────────────────────────────

    private static readonly Dictionary<string, string> AirlineAccentMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // American carriers
        { "AAL", "American" }, // American Airlines
        { "DAL", "American" }, // Delta
        { "UAL", "American" }, // United
        { "SWA", "American" }, // Southwest
        { "JBU", "American" }, // JetBlue
        { "ASA", "American" }, // Alaska Airlines
        { "FFT", "American" }, // Frontier
        { "NKS", "American" }, // Spirit
        { "FDX", "American" }, // FedEx
        { "UPS", "American" }, // UPS
        { "CGO", "American" }, // Cargo

        // British carriers
        { "BAW", "British" }, // British Airways
        { "EZY", "British" }, // EasyJet
        { "VIR", "British" }, // Virgin Atlantic

        // Irish carriers
        { "EIN", "Irish" }, // Aer Lingus
        { "RYR", "Irish" }, // Ryanair

        // French carriers
        { "AFR", "French" }, // Air France

        // German carriers
        { "DLH", "German" }, // Lufthansa
        { "EWG", "German" }, // Eurowings

        // Italian carriers
        { "AZA", "Italian" }, // ITA Airways (formerly Alitalia)

        // Spanish carriers
        { "IBE", "Spanish" }, // Iberia
        { "VLG", "Spanish" }, // Vueling

        // Dutch carriers
        { "KLM", "Dutch" }, // KLM

        // Russian carriers
        { "AFL", "Russian" }, // Aeroflot
        { "SBI", "Russian" }, // S7 Airlines

        // Polish carriers
        { "LOT", "Polish" }, // LOT Polish Airlines

        // Scandinavian carriers
        { "SAS", "Scandinavian" }, // SAS
        { "NAX", "Scandinavian" }, // Norwegian
    };

    private const string DefaultAccent = "American";

    /// <summary>
    /// Extracts the 3-letter ICAO airline prefix from a callsign (e.g. "BAW456" → "BAW").
    /// </summary>
    public static string ExtractAirlinePrefix(string callsign)
    {
        if (string.IsNullOrEmpty(callsign) || callsign.Length < 3)
            return string.Empty;

        // ICAO callsigns: 3 letter prefix followed by digits
        int letterCount = 0;
        foreach (char c in callsign)
        {
            if (char.IsLetter(c))
                letterCount++;
            else
                break;
        }

        return letterCount >= 2 ? callsign[..Math.Min(letterCount, 3)].ToUpperInvariant() : string.Empty;
    }

    /// <summary>
    /// Assigns a Piper TTS voice to a flight based on its callsign.
    /// The assignment is deterministic — the same callsign always produces the same voice.
    /// </summary>
    public static VoiceAssignment AssignVoice(string callsign)
    {
        var prefix = ExtractAirlinePrefix(callsign);
        var accent = AirlineAccentMap.GetValueOrDefault(prefix, DefaultAccent);
        var voices = VoiceCatalog[accent];

        // Deterministic selection from callsign hash
        var hash = GetStableHash(callsign);

        // Pick gender: 50/50 split
        var gender = (hash & 1) == 0 ? VoiceGender.Male : VoiceGender.Female;

        var candidates = voices.Where(v => v.Gender == gender).ToList();
        if (candidates.Count == 0)
            candidates = voices; // fallback if accent only has one gender

        var selected = candidates[Math.Abs(hash / 2) % candidates.Count];
        return new VoiceAssignment(selected.PiperVoiceName, selected.Gender, accent);
    }

    /// <summary>
    /// Returns all unique Piper voice model names used across the catalog.
    /// Useful for pre-downloading voices.
    /// </summary>
    public static IReadOnlyList<string> GetAllVoiceNames()
    {
        return VoiceCatalog.Values
            .SelectMany(v => v)
            .Select(v => v.PiperVoiceName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    /// <summary>
    /// Returns the accent for a given ICAO airline prefix, or the default accent.
    /// </summary>
    public static string GetAccentForAirline(string icaoPrefix)
    {
        return AirlineAccentMap.GetValueOrDefault(icaoPrefix.ToUpperInvariant(), DefaultAccent);
    }

    /// <summary>
    /// Stable hash that doesn't change across app restarts (unlike string.GetHashCode).
    /// </summary>
    private static int GetStableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in value)
                hash = hash * 31 + c;
            return hash;
        }
    }
}
