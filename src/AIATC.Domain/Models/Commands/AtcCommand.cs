namespace AIATC.Domain.Models.Commands;

/// <summary>
/// Base class for all ATC commands
/// </summary>
public abstract class AtcCommand
{
    public string OriginalText { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public abstract string GetReadback();
}

/// <summary>
/// Command to turn to a specific heading
/// </summary>
public class HeadingCommand : AtcCommand
{
    public float TargetHeadingDegrees { get; set; }
    public TurnDirection Direction { get; set; }

    public override string GetReadback()
    {
        var dir = Direction == TurnDirection.Left ? "left" : "right";
        return $"Turn {dir} heading {TargetHeadingDegrees:000}";
    }
}

/// <summary>
/// Command to maintain or change altitude
/// </summary>
public class AltitudeCommand : AtcCommand
{
    public float TargetAltitudeFeet { get; set; }
    public AltitudeChange ChangeType { get; set; }

    public override string GetReadback()
    {
        var action = ChangeType switch
        {
            AltitudeChange.Climb => "Climb and maintain",
            AltitudeChange.Descend => "Descend and maintain",
            AltitudeChange.Maintain => "Maintain",
            _ => "Maintain"
        };
        return $"{action} {TargetAltitudeFeet:N0}";
    }
}

/// <summary>
/// Command to change speed
/// </summary>
public class SpeedCommand : AtcCommand
{
    public float TargetSpeedKnots { get; set; }
    public SpeedChange ChangeType { get; set; }

    public override string GetReadback()
    {
        var action = ChangeType switch
        {
            SpeedChange.Increase => "Increase speed",
            SpeedChange.Reduce => "Reduce speed",
            SpeedChange.Maintain => "Maintain",
            _ => "Maintain"
        };
        return $"{action} {TargetSpeedKnots:N0} knots";
    }
}

/// <summary>
/// Command to proceed direct to a fix/waypoint
/// </summary>
public class DirectCommand : AtcCommand
{
    public string FixName { get; set; } = string.Empty;

    public override string GetReadback()
    {
        return $"Proceed direct {FixName}";
    }
}

/// <summary>
/// Command to contact another frequency (handoff)
/// </summary>
public class ContactCommand : AtcCommand
{
    public string FacilityName { get; set; } = string.Empty;
    public float FrequencyMhz { get; set; }

    public override string GetReadback()
    {
        return $"Contact {FacilityName} {FrequencyMhz:F1}";
    }
}

/// <summary>
/// Command for approach clearance
/// </summary>
public class ApproachCommand : AtcCommand
{
    public ApproachType Type { get; set; }
    public string RunwayIdentifier { get; set; } = string.Empty;

    public override string GetReadback()
    {
        var typeStr = Type switch
        {
            ApproachType.ILS => "ILS",
            ApproachType.Visual => "visual",
            ApproachType.RNAV => "RNAV",
            ApproachType.Localizer => "localizer",
            _ => ""
        };
        return $"Cleared {typeStr} approach runway {RunwayIdentifier}";
    }
}

/// <summary>
/// Command to hold at a fix
/// </summary>
public class HoldCommand : AtcCommand
{
    public string FixName { get; set; } = string.Empty;
    public float? InboundCourseDegrees { get; set; }
    public TurnDirection TurnDirection { get; set; } = TurnDirection.Right;

    public override string GetReadback()
    {
        var turns = TurnDirection == TurnDirection.Right ? "right" : "left";
        if (InboundCourseDegrees.HasValue)
        {
            return $"Hold at {FixName}, {InboundCourseDegrees:000} inbound, {turns} turns";
        }
        return $"Hold at {FixName}, {turns} turns";
    }
}

public enum TurnDirection
{
    Left,
    Right,
    Either
}

public enum AltitudeChange
{
    Climb,
    Descend,
    Maintain
}

public enum SpeedChange
{
    Increase,
    Reduce,
    Maintain
}

public enum ApproachType
{
    ILS,
    Visual,
    RNAV,
    Localizer,
    VOR,
    NDB
}
