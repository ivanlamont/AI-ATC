using System;

namespace AIATC.Domain.Models.Commands;

/// <summary>
/// Applies ATC clearances to aircraft models, converting commands to control inputs
/// </summary>
public class ClearanceApplicator
{
    /// <summary>
    /// Applies a command to an aircraft, setting appropriate targets
    /// </summary>
    public bool ApplyCommand(AtcCommand command, AircraftModel aircraft)
    {
        switch (command)
        {
            case HeadingCommand headingCmd:
                return ApplyHeadingClearance(headingCmd, aircraft);

            case AltitudeCommand altCmd:
                return ApplyAltitudeClearance(altCmd, aircraft);

            case SpeedCommand speedCmd:
                return ApplySpeedClearance(speedCmd, aircraft);

            case DirectCommand directCmd:
                return ApplyDirectClearance(directCmd, aircraft);

            case ApproachCommand approachCmd:
                return ApplyApproachClearance(approachCmd, aircraft);

            case ContactCommand contactCmd:
                return ApplyContactClearance(contactCmd, aircraft);

            case HoldCommand holdCmd:
                return ApplyHoldClearance(holdCmd, aircraft);

            default:
                return false;
        }
    }

    private bool ApplyHeadingClearance(HeadingCommand command, AircraftModel aircraft)
    {
        // Validate heading is in valid range
        if (command.TargetHeadingDegrees < 0 || command.TargetHeadingDegrees >= 360)
            return false;

        aircraft.TargetHeadingDegrees = command.TargetHeadingDegrees;
        aircraft.TargetFix = null; // Clear any direct-to navigation

        // Calculate turn rate based on direction
        var currentHeading = aircraft.HeadingDegrees;
        var targetHeading = command.TargetHeadingDegrees;

        float turnAmount = targetHeading - currentHeading;
        if (turnAmount < -180) turnAmount += 360;
        if (turnAmount > 180) turnAmount -= 360;

        // Determine turn direction
        TurnDirection actualDirection;
        if (command.Direction == TurnDirection.Either)
        {
            // Turn shortest way
            actualDirection = turnAmount > 0 ? TurnDirection.Right : TurnDirection.Left;
        }
        else
        {
            actualDirection = command.Direction;
        }

        // Apply standard rate turn (3 degrees per second)
        var turnRate = SimulationConstants.MaxTurnRate * SimulationConstants.DegreesToRadians;
        aircraft.TurnRateRadPerSec = actualDirection == TurnDirection.Right ? turnRate : -turnRate;

        return true;
    }

    private bool ApplyAltitudeClearance(AltitudeCommand command, AircraftModel aircraft)
    {
        // Validate altitude is in valid range
        if (command.TargetAltitudeFeet < 0 || command.TargetAltitudeFeet > SimulationConstants.MaxAltitude)
            return false;

        aircraft.TargetAltitudeFt = command.TargetAltitudeFeet;

        // Set appropriate vertical speed based on altitude change
        var altitudeDiff = command.TargetAltitudeFeet - aircraft.AltitudeFt;

        if (Math.Abs(altitudeDiff) < SimulationConstants.AltitudeDeadband)
        {
            // Already at target altitude
            aircraft.VerticalSpeedFpm = 0;
        }
        else if (altitudeDiff > 0)
        {
            // Need to climb
            var climbRate = Math.Min(SimulationConstants.MaxVerticalSpeed, Math.Abs(altitudeDiff) * 0.5f);
            aircraft.VerticalSpeedFpm = climbRate;
        }
        else
        {
            // Need to descend
            var descentRate = Math.Min(SimulationConstants.MaxVerticalSpeed, Math.Abs(altitudeDiff) * 0.5f);
            aircraft.VerticalSpeedFpm = -descentRate;
        }

        return true;
    }

    private bool ApplySpeedClearance(SpeedCommand command, AircraftModel aircraft)
    {
        // Validate speed is in valid range
        if (command.TargetSpeedKnots < aircraft.MinSpeedKnots ||
            command.TargetSpeedKnots > aircraft.MaxSpeedKnots)
            return false;

        aircraft.TargetSpeedKnots = command.TargetSpeedKnots;

        // Set appropriate acceleration
        var speedDiff = command.TargetSpeedKnots - aircraft.SpeedKnots;

        if (Math.Abs(speedDiff) < 5.0f)
        {
            // Already at target speed
            aircraft.AccelerationKnotsPerSec = 0;
        }
        else if (speedDiff > 0)
        {
            // Need to speed up
            aircraft.AccelerationKnotsPerSec = Math.Min(
                SimulationConstants.MaxAcceleration,
                Math.Abs(speedDiff) * 0.1f);
        }
        else
        {
            // Need to slow down
            aircraft.AccelerationKnotsPerSec = -Math.Min(
                SimulationConstants.MaxAcceleration,
                Math.Abs(speedDiff) * 0.1f);
        }

        return true;
    }

    private bool ApplyDirectClearance(DirectCommand command, AircraftModel aircraft)
    {
        // This requires a navigation system to look up the fix position
        // For now, just clear the heading target and store the fix name
        // The navigation system will handle calculating the heading to the fix

        // Note: This will be fully implemented in Task #7 (Navigation System)
        aircraft.TargetHeadingRadians = null; // Clear heading clearance

        // Store fix target (will be resolved by navigation system)
        // This is a placeholder - Task #7 will implement full fix database
        return true;
    }

    private bool ApplyApproachClearance(ApproachCommand command, AircraftModel aircraft)
    {
        // Approach clearance handling
        // This requires runway information from navigation system
        // Will be fully implemented in Task #7

        // For now, just acknowledge the clearance
        return true;
    }

    private bool ApplyContactClearance(ContactCommand command, AircraftModel aircraft)
    {
        // Contact clearance is a handoff - doesn't affect flight controls
        // This will be handled by the session/scenario management
        return true;
    }

    private bool ApplyHoldClearance(HoldCommand command, AircraftModel aircraft)
    {
        // Hold clearance requires navigation system
        // Will be fully implemented in Task #7
        return true;
    }

    /// <summary>
    /// Validates if a command can be applied to an aircraft
    /// </summary>
    public (bool valid, string? errorMessage) ValidateCommand(AtcCommand command, AircraftModel aircraft)
    {
        switch (command)
        {
            case HeadingCommand headingCmd:
                if (headingCmd.TargetHeadingDegrees < 0 || headingCmd.TargetHeadingDegrees >= 360)
                    return (false, "Heading must be between 0 and 359 degrees");
                break;

            case AltitudeCommand altCmd:
                if (altCmd.TargetAltitudeFeet < 0)
                    return (false, "Altitude cannot be negative");
                if (altCmd.TargetAltitudeFeet > SimulationConstants.MaxAltitude)
                    return (false, $"Altitude cannot exceed {SimulationConstants.MaxAltitude} feet");
                break;

            case SpeedCommand speedCmd:
                if (speedCmd.TargetSpeedKnots < aircraft.MinSpeedKnots)
                    return (false, $"Speed cannot be below {aircraft.MinSpeedKnots} knots (minimum for this aircraft)");
                if (speedCmd.TargetSpeedKnots > aircraft.MaxSpeedKnots)
                    return (false, $"Speed cannot exceed {aircraft.MaxSpeedKnots} knots (maximum for this aircraft)");
                break;
        }

        return (true, null);
    }
}
