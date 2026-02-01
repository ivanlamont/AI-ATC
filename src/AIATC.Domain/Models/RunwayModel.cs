using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Represents a runway with approach path information.
/// </summary>
public class RunwayModel
{
    public string Identifier { get; set; } = string.Empty;
    public AirportModel Airport { get; set; }

    /// <summary>
    /// Runway heading in degrees (inbound direction, direction aircraft flies TO runway)
    /// </summary>
    public float HeadingDegrees { get; set; }

    /// <summary>
    /// Distance from runway threshold to Final Approach Fix (FAF)
    /// </summary>
    public float FafDistanceNm { get; set; }

    /// <summary>
    /// Glideslope angle in degrees (typically 3.0)
    /// </summary>
    public float GlideslopeAngleDegrees { get; set; }

    /// <summary>
    /// ILS localizer frequency in MHz (if equipped)
    /// </summary>
    public float? LocalizerFrequencyMhz { get; set; }

    public bool HasIls => LocalizerFrequencyMhz.HasValue;

    private Vector2? _localizerDirection;
    private Vector2? _outboundDirection;

    public RunwayModel(AirportModel airport, string identifier, float headingDegrees, float fafDistanceNm = 6.0f)
    {
        Airport = airport;
        Identifier = identifier;
        HeadingDegrees = headingDegrees;
        FafDistanceNm = fafDistanceNm;
        GlideslopeAngleDegrees = 3.0f;
    }

    /// <summary>
    /// Unit vector pointing in localizer direction (toward runway)
    /// Aviation: 0° = North, 90° = East, 180° = South, 270° = West
    /// Trig: 0° = East, 90° = North, 180° = West, 270° = South
    /// Conversion: trig_angle = 90 - aviation_angle
    /// </summary>
    public Vector2 LocalizerDirection
    {
        get
        {
            if (!_localizerDirection.HasValue)
            {
                var trigAngleRad = (90 - HeadingDegrees) * SimulationConstants.DegreesToRadians;
                _localizerDirection = new Vector2(
                    MathF.Cos(trigAngleRad),
                    MathF.Sin(trigAngleRad)
                );
            }
            return _localizerDirection.Value;
        }
    }

    /// <summary>
    /// Unit vector pointing away from runway (outbound direction)
    /// </summary>
    public Vector2 OutboundDirection
    {
        get
        {
            if (!_outboundDirection.HasValue)
            {
                _outboundDirection = -LocalizerDirection;
            }
            return _outboundDirection.Value;
        }
    }

    /// <summary>
    /// Gets position along localizer at given distance FROM runway threshold
    /// </summary>
    public Vector2 GetLocalizerPoint(float distanceFromThresholdNm)
    {
        return Airport.PositionNm + OutboundDirection * distanceFromThresholdNm;
    }

    /// <summary>
    /// Gets Final Approach Fix position
    /// </summary>
    public Vector2 GetFafPosition()
    {
        return GetLocalizerPoint(FafDistanceNm);
    }

    /// <summary>
    /// Calculates ideal altitude at given distance from threshold (on glideslope)
    /// </summary>
    public float GetGlideslopeAltitude(float distanceFromThresholdNm)
    {
        // Standard 3-degree glideslope = 318 ft/nm
        var glideslopeFtPerNm = MathF.Tan(GlideslopeAngleDegrees * SimulationConstants.DegreesToRadians) * 6076.12f;
        return Airport.AltitudeFt + distanceFromThresholdNm * glideslopeFtPerNm;
    }

    /// <summary>
    /// Calculates distance along localizer from threshold (negative = past threshold)
    /// </summary>
    public float GetDistanceAlongLocalizer(Vector2 position)
    {
        var relativePos = position - Airport.PositionNm;
        return Vector2.Dot(relativePos, OutboundDirection);
    }

    /// <summary>
    /// Calculates perpendicular distance from localizer centerline (+ = right of course)
    /// </summary>
    public float GetDeviationFromLocalizer(Vector2 position)
    {
        var relativePos = position - Airport.PositionNm;
        var perpendicular = new Vector2(-OutboundDirection.Y, OutboundDirection.X);
        return Vector2.Dot(relativePos, perpendicular);
    }
}
