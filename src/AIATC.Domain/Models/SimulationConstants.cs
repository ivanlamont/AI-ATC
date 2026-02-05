using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Constants for aircraft simulation physics.
/// Units: NM (nautical miles), knots, feet, radians
/// </summary>
public static class SimulationConstants
{
    // Altitude limits (feet)
    public const float MaxVerticalSpeed = 3000.0f;      // ft/min
    public const float MaxVerticalAccel = 1000.0f;      // ft/min^2
    public const float MinAltitude = 0.0f;              // ft (ground level)
    public const float MaxAltitude = 40000.0f;          // ft (ceiling)

    // Control gains (tuned)
    public const float AltitudeKp = 0.002f;             // Proportional gain for altitude
    public const float HeadingKp = 0.8f;                // Proportional gain for heading
    public const float SpeedKp = 0.02f;                 // Proportional gain for speed

    // Approach parameters
    public const float AltitudeDeadband = 100.0f;       // ft
    public const float ApproachSpeed = 150.0f;          // knots
    public const float TerminalRadius = 20.0f;          // NM

    // Standard glideslope (3 degrees = 318 ft/nm)
    public const float StandardGlideslopeFtPerNm = 318.0f;

    // Maximum rates
    public const float MaxTurnRate = 3.0f;              // degrees/sec
    public const float MaxAcceleration = 5.0f;          // knots/sec

    // Clearance timing
    public const float ClearanceIntervalSeconds = 15.0f;

    // Separation minimums
    public const float MinimumSeparationNm = 3.0f;      // NM (lateral)
    public const float MinimumVerticalSeparationFt = 1000.0f; // ft

    // Landing criteria
    public const float LandingRadiusNm = 2.0f;          // NM
    public const float LandingMaxAltitudeFt = 1500.0f;
    public const float LandingMaxVerticalSpeedFpm = 700.0f;
    public const float LandingMaxTurnRateDegPerSec = 3.0f;

    // Earth radius for coordinate conversions
    public const double EarthRadiusMeters = 6378137.0;

    // Conversion factors
    public const float DegreesToRadians = MathF.PI / 180.0f;
    public const float RadiansToDegrees = 180.0f / MathF.PI;
    public const float KnotsToNmPerSecond = 1.0f / 3600.0f;
    public const float FeetPerMinuteToFeetPerSecond = 1.0f / 60.0f;
}
