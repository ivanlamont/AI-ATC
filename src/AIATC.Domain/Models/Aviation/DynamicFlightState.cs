using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Dynamic flight state with physics modeling and real-time tracking
/// </summary>
public class DynamicFlightState
{
    /// <summary>
    /// Current position in nautical miles
    /// </summary>
    public Vector2 PositionNm { get; set; } = new Vector2(0, 0);

    /// <summary>
    /// Velocity vector in knots
    /// </summary>
    public Vector2 VelocityKnots { get; set; } = new Vector2(0, 0);

    /// <summary>
    /// Acceleration vector in knots per second
    /// </summary>
    public Vector2 AccelerationKnotsPerSec { get; set; } = new Vector2(0, 0);

    /// <summary>
    /// Current heading in radians
    /// </summary>
    public float HeadingRadians { get; set; }

    /// <summary>
    /// Ground speed in knots
    /// </summary>
    public float SpeedKnots { get; set; } = 220;

    /// <summary>
    /// Indicated airspeed in knots
    /// </summary>
    public float IndicatedAirspeedKnots { get; set; } = 220;

    /// <summary>
    /// True airspeed in knots
    /// </summary>
    public float TrueAirspeedKnots { get; set; } = 220;

    /// <summary>
    /// Current altitude in feet MSL
    /// </summary>
    public float AltitudeFt { get; set; } = 5000;

    /// <summary>
    /// Vertical speed in feet per minute
    /// </summary>
    public float VerticalSpeedFpm { get; set; }

    /// <summary>
    /// Current turn rate in radians per second
    /// </summary>
    public float TurnRateRadPerSec { get; set; }

    /// <summary>
    /// Track angle in radians (heading + wind correction)
    /// </summary>
    public float TrackRadians => HeadingRadians; // Simplified for now

    /// <summary>
    /// Bank angle in degrees (for turn visualization)
    /// </summary>
    public float BankAngleDegrees { get; set; }

    /// <summary>
    /// Position history for trail display
    /// </summary>
    public List<Vector2> PositionHistory { get; set; } = new();

    /// <summary>
    /// Maximum history points to retain
    /// </summary>
    public int MaxHistoryPoints { get; set; } = 50;

    /// <summary>
    /// Last state update timestamp
    /// </summary>
    public DateTime LastUpdateUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Update position history
    /// </summary>
    public void UpdateHistory()
    {
        PositionHistory.Add(PositionNm);
        
        if (PositionHistory.Count > MaxHistoryPoints)
        {
            PositionHistory.RemoveAt(0);
        }
        
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate distance traveled since last update
    /// </summary>
    public float GetDistanceTraveledNm()
    {
        if (PositionHistory.Count < 2)
            return 0;
            
        var prevPos = PositionHistory[^2];
        return (PositionNm - prevPos).Magnitude;
    }
}