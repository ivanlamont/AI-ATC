using System.Collections.Generic;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Represents a published instrument procedure (SID, STAR, or Approach)
/// </summary>
public class Procedure
{
    /// <summary>
    /// Procedure identifier (e.g., "BGGLO2", "DYAMD3", "ILS27")
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Type of procedure
    /// </summary>
    public ProcedureType Type { get; set; }

    /// <summary>
    /// Airport this procedure is associated with
    /// </summary>
    public string AirportIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Runway identifier (for approaches, empty for SIDs/STARs)
    /// </summary>
    public string? RunwayIdentifier { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Route defining the procedure
    /// </summary>
    public Route Route { get; set; } = new();

    /// <summary>
    /// Initial fix for the procedure
    /// </summary>
    public Fix? InitialFix { get; set; }

    /// <summary>
    /// Final fix for the procedure
    /// </summary>
    public Fix? FinalFix { get; set; }

    /// <summary>
    /// Transition routes (different entry/exit points)
    /// </summary>
    public List<ProcedureTransition> Transitions { get; set; } = new();
}

/// <summary>
/// Represents a transition to/from a procedure
/// </summary>
public class ProcedureTransition
{
    /// <summary>
    /// Transition identifier (fix name)
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Route for this transition
    /// </summary>
    public Route Route { get; set; } = new();
}

/// <summary>
/// Type of instrument procedure
/// </summary>
public enum ProcedureType
{
    SID,        // Standard Instrument Departure
    STAR,       // Standard Terminal Arrival Route
    Approach    // Instrument Approach Procedure
}
