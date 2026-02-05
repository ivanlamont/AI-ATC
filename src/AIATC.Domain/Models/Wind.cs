using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Represents wind conditions affecting aircraft.
/// Wind varies by altitude layer.
/// </summary>
public class Wind
{
    /// <summary>
    /// Wind direction in degrees (direction FROM which wind is blowing)
    /// </summary>
    public float DirectionDegrees { get; set; }

    /// <summary>
    /// Wind speed in knots
    /// </summary>
    public float SpeedKnots { get; set; }

    /// <summary>
    /// Altitude layer bottom (feet)
    /// </summary>
    public float AltitudeFloorFt { get; set; }

    /// <summary>
    /// Altitude layer top (feet)
    /// </summary>
    public float AltitudeCeilingFt { get; set; }

    public Wind()
    {
        DirectionDegrees = 0;
        SpeedKnots = 0;
        AltitudeFloorFt = 0;
        AltitudeCeilingFt = SimulationConstants.MaxAltitude;
    }

    public Wind(float directionDegrees, float speedKnots, float altitudeFloorFt = 0, float altitudeCeilingFt = 40000)
    {
        DirectionDegrees = directionDegrees;
        SpeedKnots = speedKnots;
        AltitudeFloorFt = altitudeFloorFt;
        AltitudeCeilingFt = altitudeCeilingFt;
    }

    /// <summary>
    /// Gets wind velocity vector (in NM/sec, pointing in direction wind is blowing TO)
    /// </summary>
    public Vector2 GetWindVelocityNmPerSec()
    {
        // Wind direction is FROM, so add 180 to get TO direction
        // Aviation convention: 0° = North, 90° = East, etc.
        // But trigonometry: 0° = East, 90° = North
        // So we need to convert: aviation_angle = 90 - trig_angle
        var toDirection = (DirectionDegrees + 180) % 360;
        var trigAngleRad = (90 - toDirection) * SimulationConstants.DegreesToRadians;
        var speedNmPerSec = SpeedKnots * SimulationConstants.KnotsToNmPerSecond;

        return new Vector2(
            speedNmPerSec * MathF.Cos(trigAngleRad),
            speedNmPerSec * MathF.Sin(trigAngleRad)
        );
    }

    /// <summary>
    /// Calculates crosswind component for a given runway heading
    /// </summary>
    /// <param name="runwayHeadingDegrees">Runway heading (direction aircraft points on final)</param>
    /// <returns>Crosswind in knots (positive = right crosswind)</returns>
    public float GetCrosswindComponent(float runwayHeadingDegrees)
    {
        var windAngleRad = DirectionDegrees * SimulationConstants.DegreesToRadians;
        var runwayAngleRad = runwayHeadingDegrees * SimulationConstants.DegreesToRadians;
        var relativeAngle = windAngleRad - runwayAngleRad;

        return SpeedKnots * MathF.Sin(relativeAngle);
    }

    /// <summary>
    /// Calculates headwind component for a given runway heading
    /// </summary>
    /// <param name="runwayHeadingDegrees">Runway heading (direction aircraft points)</param>
    /// <returns>Headwind in knots (positive = headwind, negative = tailwind)</returns>
    public float GetHeadwindComponent(float runwayHeadingDegrees)
    {
        // Wind FROM direction minus runway heading
        var relativeAngle = (DirectionDegrees - runwayHeadingDegrees) * SimulationConstants.DegreesToRadians;

        // Positive when wind is FROM ahead (headwind)
        return SpeedKnots * MathF.Cos(relativeAngle);
    }

    public static Wind Calm => new(0, 0);
}
