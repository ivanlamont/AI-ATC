using System;

namespace AIATC.Domain.Models.Weather;

/// <summary>
/// Represents visibility conditions
/// </summary>
public class VisibilityConditions
{
    /// <summary>
    /// Visibility distance in statute miles
    /// </summary>
    public float VisibilityMiles { get; set; }

    /// <summary>
    /// Precipitation type
    /// </summary>
    public PrecipitationType Precipitation { get; set; }

    /// <summary>
    /// Intensity of precipitation
    /// </summary>
    public PrecipitationIntensity Intensity { get; set; }

    /// <summary>
    /// Obscuration conditions (fog, mist, haze)
    /// </summary>
    public ObscurationType Obscuration { get; set; }

    /// <summary>
    /// Checks if conditions are VMC (Visual Meteorological Conditions)
    /// </summary>
    public bool IsVmc(float altitudeFt)
    {
        // Simplified VMC criteria
        if (altitudeFt >= 10000)
        {
            // Class E high altitude: 5 SM, 1000' below, 1000' above, 1 SM horizontal
            return VisibilityMiles >= 5.0f;
        }
        else if (altitudeFt >= 1200)
        {
            // Class E low altitude: 3 SM, 500' below, 1000' above, 2000' horizontal
            return VisibilityMiles >= 3.0f;
        }
        else
        {
            // Class G surface: 1 SM by day, 3 SM by night
            return VisibilityMiles >= 1.0f;
        }
    }

    /// <summary>
    /// Checks if conditions are IFR (Instrument Flight Rules)
    /// </summary>
    public bool IsIfr(float? ceilingFt)
    {
        // IFR: Ceiling < 1000 ft AGL or visibility < 3 SM
        return VisibilityMiles < 3.0f || (ceilingFt.HasValue && ceilingFt.Value < 1000);
    }

    /// <summary>
    /// Checks if conditions are LIFR (Low IFR)
    /// </summary>
    public bool IsLifr(float? ceilingFt)
    {
        // LIFR: Ceiling < 500 ft AGL or visibility < 1 SM
        return VisibilityMiles < 1.0f || (ceilingFt.HasValue && ceilingFt.Value < 500);
    }

    /// <summary>
    /// Checks if conditions are MVFR (Marginal VFR)
    /// </summary>
    public bool IsMvfr(float? ceilingFt)
    {
        // MVFR: Ceiling 1000-3000 ft AGL or visibility 3-5 SM
        var ceilingMvfr = ceilingFt.HasValue && ceilingFt.Value >= 1000 && ceilingFt.Value <= 3000;
        var visMvfr = VisibilityMiles >= 3.0f && VisibilityMiles <= 5.0f;

        return ceilingMvfr || visMvfr;
    }

    /// <summary>
    /// Gets flight category string
    /// </summary>
    public string GetFlightCategory(float? ceilingFt)
    {
        if (IsLifr(ceilingFt)) return "LIFR";
        if (IsIfr(ceilingFt)) return "IFR";
        if (IsMvfr(ceilingFt)) return "MVFR";
        return "VFR";
    }

    /// <summary>
    /// Formats visibility as METAR style
    /// </summary>
    public string ToMetarString()
    {
        var vis = VisibilityMiles >= 10 ? "10SM" : $"{VisibilityMiles:F0}SM";

        var wx = "";

        // Add intensity prefix
        if (Intensity == PrecipitationIntensity.Light)
            wx += "-";
        else if (Intensity == PrecipitationIntensity.Heavy)
            wx += "+";

        // Add precipitation
        wx += Precipitation switch
        {
            PrecipitationType.Rain => "RA",
            PrecipitationType.Snow => "SN",
            PrecipitationType.Drizzle => "DZ",
            PrecipitationType.Freezing => "FZRA",
            PrecipitationType.Thunderstorm => "TSRA",
            _ => ""
        };

        // Add obscuration
        wx += Obscuration switch
        {
            ObscurationType.Fog => "FG",
            ObscurationType.Mist => "BR",
            ObscurationType.Haze => "HZ",
            ObscurationType.Smoke => "FU",
            _ => ""
        };

        return string.IsNullOrEmpty(wx) ? vis : $"{vis} {wx}";
    }

    /// <summary>
    /// Creates clear visibility conditions
    /// </summary>
    public static VisibilityConditions CreateClear()
    {
        return new VisibilityConditions
        {
            VisibilityMiles = 10,
            Precipitation = PrecipitationType.None,
            Intensity = PrecipitationIntensity.None,
            Obscuration = ObscurationType.None
        };
    }

    /// <summary>
    /// Creates IFR conditions
    /// </summary>
    public static VisibilityConditions CreateIfr()
    {
        return new VisibilityConditions
        {
            VisibilityMiles = 2,
            Precipitation = PrecipitationType.Rain,
            Intensity = PrecipitationIntensity.Moderate,
            Obscuration = ObscurationType.Mist
        };
    }
}

/// <summary>
/// Precipitation types
/// </summary>
public enum PrecipitationType
{
    None,
    Rain,
    Snow,
    Drizzle,
    Freezing,
    Thunderstorm
}

/// <summary>
/// Precipitation intensity levels
/// </summary>
public enum PrecipitationIntensity
{
    None,
    Light,
    Moderate,
    Heavy
}

/// <summary>
/// Obscuration types
/// </summary>
public enum ObscurationType
{
    None,
    Fog,
    Mist,
    Haze,
    Smoke
}
