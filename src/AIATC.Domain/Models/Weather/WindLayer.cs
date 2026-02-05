using System;

namespace AIATC.Domain.Models.Weather;

/// <summary>
/// Represents wind conditions at a specific altitude layer
/// </summary>
public class WindLayer
{
    /// <summary>
    /// Wind direction in degrees (0-359, 0 = North, aviation convention)
    /// </summary>
    public float DirectionDegrees { get; set; }

    /// <summary>
    /// Sustained wind speed in knots
    /// </summary>
    public float SpeedKnots { get; set; }

    /// <summary>
    /// Gust speed in knots (0 if no gusts)
    /// </summary>
    public float GustKnots { get; set; }

    /// <summary>
    /// Base altitude of this layer in feet MSL
    /// </summary>
    public float BaseAltitudeFt { get; set; }

    /// <summary>
    /// Top altitude of this layer in feet MSL
    /// </summary>
    public float TopAltitudeFt { get; set; }

    /// <summary>
    /// Gets the wind vector (east-west, north-south components) in knots
    /// </summary>
    public Vector2 GetWindVector()
    {
        // Convert aviation degrees (0=North, clockwise) to radians
        var radians = (90 - DirectionDegrees) * MathF.PI / 180f;

        return new Vector2(
            SpeedKnots * MathF.Cos(radians),  // East component
            SpeedKnots * MathF.Sin(radians)   // North component
        );
    }

    /// <summary>
    /// Gets the current wind speed (with chance of gusts)
    /// </summary>
    public float GetCurrentSpeed(Random? random = null)
    {
        if (GustKnots <= SpeedKnots)
            return SpeedKnots;

        random ??= Random.Shared;

        // 20% chance of gust
        if (random.NextDouble() < 0.2)
        {
            // Random gust between sustained and max gust
            return SpeedKnots + (float)random.NextDouble() * (GustKnots - SpeedKnots);
        }

        return SpeedKnots;
    }

    /// <summary>
    /// Checks if an altitude is within this wind layer
    /// </summary>
    public bool ContainsAltitude(float altitudeFt)
    {
        return altitudeFt >= BaseAltitudeFt && altitudeFt <= TopAltitudeFt;
    }

    /// <summary>
    /// Creates a surface wind layer (0-3000 ft)
    /// </summary>
    public static WindLayer CreateSurface(float directionDegrees, float speedKnots, float gustKnots = 0)
    {
        return new WindLayer
        {
            DirectionDegrees = directionDegrees,
            SpeedKnots = speedKnots,
            GustKnots = gustKnots,
            BaseAltitudeFt = 0,
            TopAltitudeFt = 3000
        };
    }

    /// <summary>
    /// Creates a winds aloft layer
    /// </summary>
    public static WindLayer CreateAloft(float directionDegrees, float speedKnots,
        float baseAltitudeFt, float topAltitudeFt)
    {
        return new WindLayer
        {
            DirectionDegrees = directionDegrees,
            SpeedKnots = speedKnots,
            GustKnots = 0,  // Rarely gusts aloft
            BaseAltitudeFt = baseAltitudeFt,
            TopAltitudeFt = topAltitudeFt
        };
    }

    /// <summary>
    /// Formats wind as METAR style (e.g., "27015G25KT")
    /// </summary>
    public string ToMetarString()
    {
        var dir = ((int)DirectionDegrees).ToString("D3");
        var speed = ((int)SpeedKnots).ToString("D2");

        if (GustKnots > SpeedKnots)
        {
            var gust = ((int)GustKnots).ToString("D2");
            return $"{dir}{speed}G{gust}KT";
        }

        return $"{dir}{speed}KT";
    }
}
