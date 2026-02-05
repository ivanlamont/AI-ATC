using System;

namespace AIATC.Domain.Models.Weather;

/// <summary>
/// Represents a cloud layer with base and top altitudes
/// </summary>
public class CloudLayer
{
    /// <summary>
    /// Cloud coverage type
    /// </summary>
    public CloudCoverage Coverage { get; set; }

    /// <summary>
    /// Base altitude of clouds in feet AGL
    /// </summary>
    public float BaseAltitudeAgl { get; set; }

    /// <summary>
    /// Top altitude of clouds in feet AGL
    /// </summary>
    public float TopAltitudeAgl { get; set; }

    /// <summary>
    /// Cloud type
    /// </summary>
    public CloudType Type { get; set; }

    /// <summary>
    /// Checks if an altitude is within this cloud layer
    /// </summary>
    public bool ContainsAltitude(float altitudeAgl)
    {
        return altitudeAgl >= BaseAltitudeAgl && altitudeAgl <= TopAltitudeAgl;
    }

    /// <summary>
    /// Checks if this layer defines the ceiling
    /// </summary>
    public bool IsCeiling()
    {
        return Coverage == CloudCoverage.Broken || Coverage == CloudCoverage.Overcast;
    }

    /// <summary>
    /// Formats cloud layer as METAR style (e.g., "BKN025")
    /// </summary>
    public string ToMetarString()
    {
        var coverage = Coverage switch
        {
            CloudCoverage.Clear => "CLR",
            CloudCoverage.Few => "FEW",
            CloudCoverage.Scattered => "SCT",
            CloudCoverage.Broken => "BKN",
            CloudCoverage.Overcast => "OVC",
            _ => "SKC"
        };

        if (Coverage == CloudCoverage.Clear)
            return coverage;

        var altitude = ((int)(BaseAltitudeAgl / 100)).ToString("D3");
        return $"{coverage}{altitude}";
    }

    /// <summary>
    /// Creates a ceiling layer
    /// </summary>
    public static CloudLayer CreateCeiling(float baseAgl, CloudCoverage coverage = CloudCoverage.Overcast)
    {
        return new CloudLayer
        {
            Coverage = coverage,
            BaseAltitudeAgl = baseAgl,
            TopAltitudeAgl = baseAgl + 2000,  // Typical thickness
            Type = CloudType.Stratus
        };
    }

    /// <summary>
    /// Creates a scattered layer
    /// </summary>
    public static CloudLayer CreateScattered(float baseAgl, float thicknessFt = 1000)
    {
        return new CloudLayer
        {
            Coverage = CloudCoverage.Scattered,
            BaseAltitudeAgl = baseAgl,
            TopAltitudeAgl = baseAgl + thicknessFt,
            Type = CloudType.Cumulus
        };
    }
}

/// <summary>
/// Cloud coverage amount
/// </summary>
public enum CloudCoverage
{
    /// <summary>
    /// Clear skies (0 oktas)
    /// </summary>
    Clear = 0,

    /// <summary>
    /// Few clouds (1-2 oktas)
    /// </summary>
    Few = 1,

    /// <summary>
    /// Scattered clouds (3-4 oktas)
    /// </summary>
    Scattered = 2,

    /// <summary>
    /// Broken clouds (5-7 oktas) - defines ceiling
    /// </summary>
    Broken = 3,

    /// <summary>
    /// Overcast (8 oktas) - defines ceiling
    /// </summary>
    Overcast = 4
}

/// <summary>
/// Cloud types
/// </summary>
public enum CloudType
{
    /// <summary>
    /// Cumulus - puffy fair weather clouds
    /// </summary>
    Cumulus,

    /// <summary>
    /// Stratus - layered clouds
    /// </summary>
    Stratus,

    /// <summary>
    /// Cumulonimbus - thunderstorm clouds
    /// </summary>
    Cumulonimbus,

    /// <summary>
    /// Cirrus - high altitude ice clouds
    /// </summary>
    Cirrus
}
