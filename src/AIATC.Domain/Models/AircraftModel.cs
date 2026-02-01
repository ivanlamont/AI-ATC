using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Represents an aircraft in the simulation with full physics modeling.
/// Includes wind effects, performance envelopes, and realistic control dynamics.
/// </summary>
public class AircraftModel
{
    // Identity
    public string Callsign { get; set; } = string.Empty;
    public string AircraftType { get; set; } = "B738"; // Default Boeing 737-800
    public bool IsArrival { get; set; }

    // Current state (truth)
    public Vector2 PositionNm { get; set; }
    public float TotalDistanceFlown { get; set; } = 0;
    public float HeadingRadians { get; set; }
    public float SpeedKnots { get; set; }
    public float AltitudeFt { get; set; }
    public float VerticalSpeedFpm { get; set; }
    public float TurnRateRadPerSec { get; set; }
    public float AccelerationKnotsPerSec { get; set; }

    // Target clearances (from ATC)
    public float? TargetHeadingRadians { get; set; }
    public float? TargetSpeedKnots { get; set; }
    public float? TargetAltitudeFt { get; set; }
    public Vector2? TargetFix { get; set; }

    // Performance envelope
    public float MinSpeedKnots { get; set; }
    public float MaxSpeedKnots { get; set; }
    public float MaxTurnRateRadPerSec { get; set; }

    // Destination
    public AirportModel? Destination { get; set; }

    // Status flags
    public bool Landed { get; set; }
    public float LastClearanceTime { get; set; }

    // Tracking
    public float PreviousDistanceToDestinationNm { get; set; }
    public float DistanceToFafNm { get; set; }

    // Wind (external)
    private Wind? _currentWind;

    public AircraftModel()
    {
        PositionNm = new Vector2(0, 0);
        HeadingRadians = 0;
        SpeedKnots = 220;
        AltitudeFt = 5000;
        VerticalSpeedFpm = 0;
        TurnRateRadPerSec = 0;
        AccelerationKnotsPerSec = 0;

        MinSpeedKnots = 160;
        MaxSpeedKnots = 260;
        MaxTurnRateRadPerSec = SimulationConstants.MaxTurnRate * SimulationConstants.DegreesToRadians;

        TargetHeadingRadians = HeadingRadians;
        TargetSpeedKnots = SpeedKnots;
        TargetAltitudeFt = AltitudeFt;

        PreviousDistanceToDestinationNm = float.PositiveInfinity;
        DistanceToFafNm = float.PositiveInfinity;
    }

    public AircraftModel(
        string callsign,
        Vector2 positionNm,
        float headingRadians,
        float speedKnots,
        float altitudeFt,
        AirportModel? destination,
        bool isArrival = true)
    {
        Callsign = callsign;
        PositionNm = positionNm;
        HeadingRadians = headingRadians;
        SpeedKnots = speedKnots;
        AltitudeFt = altitudeFt;
        Destination = destination;
        IsArrival = isArrival;

        VerticalSpeedFpm = 0;
        TurnRateRadPerSec = 0;
        AccelerationKnotsPerSec = 0;

        MinSpeedKnots = 160;
        MaxSpeedKnots = 260;
        MaxTurnRateRadPerSec = SimulationConstants.MaxTurnRate * SimulationConstants.DegreesToRadians;

        TargetHeadingRadians = headingRadians;
        TargetSpeedKnots = speedKnots;
        TargetAltitudeFt = altitudeFt;

        PreviousDistanceToDestinationNm = float.PositiveInfinity;
        DistanceToFafNm = float.PositiveInfinity;
    }

    /// <summary>
    /// Apply ATC clearance commands (turn rate, acceleration, vertical speed)
    /// </summary>
    public void ApplyAtcClearance(float turnRateCommand, float accelCommand, float verticalSpeedCommand)
    {
        // Clamp to aircraft limits
        TurnRateRadPerSec = Math.Clamp(turnRateCommand, -MaxTurnRateRadPerSec, MaxTurnRateRadPerSec);
        AccelerationKnotsPerSec = Math.Clamp(accelCommand, -SimulationConstants.MaxAcceleration, SimulationConstants.MaxAcceleration);
        VerticalSpeedFpm = Math.Clamp(verticalSpeedCommand, -SimulationConstants.MaxVerticalSpeed, SimulationConstants.MaxVerticalSpeed);
    }

    /// <summary>
    /// Sets wind conditions affecting this aircraft
    /// </summary>
    public void SetWind(Wind? wind)
    {
        _currentWind = wind;
    }

    /// <summary>
    /// Physics update step
    /// </summary>
    public void Step(float deltaTimeSeconds)
    {
        if (Landed)
            return;

        // Speed integration (airspeed)
        SpeedKnots += AccelerationKnotsPerSec * deltaTimeSeconds;
        SpeedKnots = Math.Clamp(SpeedKnots, MinSpeedKnots, MaxSpeedKnots);

        // Heading integration
        HeadingRadians += TurnRateRadPerSec * deltaTimeSeconds;
        HeadingRadians = WrapAngle(HeadingRadians);

        // Altitude integration
        AltitudeFt += (VerticalSpeedFpm * SimulationConstants.FeetPerMinuteToFeetPerSecond) * deltaTimeSeconds;
        AltitudeFt = Math.Clamp(AltitudeFt, SimulationConstants.MinAltitude, SimulationConstants.MaxAltitude);

        // Lateral position integration (with wind effects)
        UpdatePosition(deltaTimeSeconds);
    }

    private void UpdatePosition(float deltaTimeSeconds)
    {
        // Aircraft velocity (airspeed in heading direction)
        // Aviation heading: 0° = North, 90° = East
        // Trig: 0° = East, 90° = North
        // Conversion: trig_angle = 90 - aviation_angle
        var airspeedNmPerSec = SpeedKnots * SimulationConstants.KnotsToNmPerSecond;
        var trigAngleRad = (90 * SimulationConstants.DegreesToRadians) - HeadingRadians;
        var aircraftVelocity = new Vector2(
            airspeedNmPerSec * MathF.Cos(trigAngleRad),
            airspeedNmPerSec * MathF.Sin(trigAngleRad)
        );

        // Add wind effect (ground speed = airspeed + wind)
        Vector2 groundVelocity;
        if (_currentWind != null &&
            AltitudeFt >= _currentWind.AltitudeFloorFt &&
            AltitudeFt <= _currentWind.AltitudeCeilingFt)
        {
            var windVelocity = _currentWind.GetWindVelocityNmPerSec();
            groundVelocity = aircraftVelocity + windVelocity;
        }
        else
        {
            groundVelocity = aircraftVelocity;
        }

        // Update position
        PositionNm += groundVelocity * deltaTimeSeconds;
        TotalDistanceFlown += groundVelocity.Magnitude * deltaTimeSeconds;
    }

    /// <summary>
    /// Calculates ground track (direction of movement over ground) accounting for wind
    /// </summary>
    public float GetGroundTrackRadians()
    {
        if (_currentWind == null || _currentWind.SpeedKnots < 1.0f)
            return HeadingRadians;

        var airspeedNmPerSec = SpeedKnots * SimulationConstants.KnotsToNmPerSecond;
        var trigAngleRad = (90 * SimulationConstants.DegreesToRadians) - HeadingRadians;
        var aircraftVelocity = new Vector2(
            airspeedNmPerSec * MathF.Cos(trigAngleRad),
            airspeedNmPerSec * MathF.Sin(trigAngleRad)
        );

        var windVelocity = _currentWind.GetWindVelocityNmPerSec();
        var groundVelocity = aircraftVelocity + windVelocity;

        // Convert back from trig angle to aviation heading
        var trigAngle = groundVelocity.ToAngleRadians();
        return (90 * SimulationConstants.DegreesToRadians) - trigAngle;
    }

    /// <summary>
    /// Gets ground speed in knots (accounting for wind)
    /// </summary>
    public float GetGroundSpeedKnots()
    {
        if (_currentWind == null || _currentWind.SpeedKnots < 1.0f)
            return SpeedKnots;

        var airspeedNmPerSec = SpeedKnots * SimulationConstants.KnotsToNmPerSecond;
        var trigAngleRad = (90 * SimulationConstants.DegreesToRadians) - HeadingRadians;
        var aircraftVelocity = new Vector2(
            airspeedNmPerSec * MathF.Cos(trigAngleRad),
            airspeedNmPerSec * MathF.Sin(trigAngleRad)
        );

        var windVelocity = _currentWind.GetWindVelocityNmPerSec();
        var groundVelocity = aircraftVelocity + windVelocity;

        return groundVelocity.Magnitude / SimulationConstants.KnotsToNmPerSecond;
    }

    /// <summary>
    /// Checks if aircraft meets landing criteria
    /// </summary>
    public bool CheckLanding(AirportModel airport, float landingRadiusNm)
    {
        if (Landed)
            return false;

        var distanceToAirport = Vector2.Distance(PositionNm, airport.PositionNm);

        bool meetsLandingCriteria =
            distanceToAirport <= landingRadiusNm &&
            AltitudeFt <= SimulationConstants.LandingMaxAltitudeFt &&
            MathF.Abs(VerticalSpeedFpm) <= SimulationConstants.LandingMaxVerticalSpeedFpm &&
            MathF.Abs(TurnRateRadPerSec) <= SimulationConstants.LandingMaxTurnRateDegPerSec * SimulationConstants.DegreesToRadians &&
            SpeedKnots <= SimulationConstants.ApproachSpeed;

        if (meetsLandingCriteria)
        {
            Landed = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates distance to destination airport
    /// </summary>
    public float GetDistanceToDestinationNm()
    {
        if (Destination == null)
            return float.PositiveInfinity;

        return Vector2.Distance(PositionNm, Destination.PositionNm);
    }

    /// <summary>
    /// Calculates heading error in degrees (for display)
    /// </summary>
    public float GetHeadingErrorDegrees()
    {
        if (!TargetHeadingRadians.HasValue)
            return 0;

        var error = TargetHeadingRadians.Value - HeadingRadians;
        error = WrapAngle(error);

        // Convert to degrees and wrap to [-180, 180]
        var errorDeg = error * SimulationConstants.RadiansToDegrees;
        if (errorDeg > 180)
            errorDeg -= 360;
        else if (errorDeg < -180)
            errorDeg += 360;

        return errorDeg;
    }

    /// <summary>
    /// Checks if aircraft is above glideslope for given runway
    /// </summary>
    public float GetGlideslopeDeviation(RunwayModel runway)
    {
        var distanceFromThreshold = runway.GetDistanceAlongLocalizer(PositionNm);
        if (distanceFromThreshold < 0)
            return 0; // Past threshold

        var idealAltitude = runway.GetGlideslopeAltitude(distanceFromThreshold);
        return AltitudeFt - idealAltitude;
    }

    /// <summary>
    /// Gets wind correction angle needed for straight ground track
    /// </summary>
    public float GetWindCorrectionAngleRadians()
    {
        if (_currentWind == null || _currentWind.SpeedKnots < 1.0f)
            return 0;

        // Simple wind correction angle calculation
        var windAngleRad = _currentWind.DirectionDegrees * SimulationConstants.DegreesToRadians;
        var relativeWindAngle = windAngleRad - HeadingRadians;
        var crosswind = _currentWind.SpeedKnots * MathF.Sin(relativeWindAngle);

        // WCA ≈ arcsin(crosswind / groundspeed)
        var groundSpeed = GetGroundSpeedKnots();
        if (groundSpeed > 0)
        {
            return MathF.Asin(Math.Clamp(crosswind / groundSpeed, -1.0f, 1.0f));
        }

        return 0;
    }

    private static float WrapAngle(float angleRadians)
    {
        var result = angleRadians % (2 * MathF.PI);
        if (result < 0)
            result += 2 * MathF.PI;
        return result;
    }

    public float HeadingDegrees
    {
        get => HeadingRadians * SimulationConstants.RadiansToDegrees;
        set => HeadingRadians = value * SimulationConstants.DegreesToRadians;
    }

    public float? TargetHeadingDegrees
    {
        get => TargetHeadingRadians.HasValue ? TargetHeadingRadians.Value * SimulationConstants.RadiansToDegrees : null;
        set => TargetHeadingRadians = value.HasValue ? value.Value * SimulationConstants.DegreesToRadians : null;
    }
}
