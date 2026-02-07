namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Aircraft operational mode for different flight phases
/// </summary>
public enum AircraftMode
{
    /// <summary>
    /// Taxi operations on ground
    /// </summary>
    Taxi,
    
    /// <summary>
    /// Takeoff and initial climb
    /// </summary>
    Departure,
    
    /// <summary>
    /// Enroute cruise flight
    /// </summary>
    Enroute,
    
    /// <summary>
    /// Terminal area operations
    /// </summary>
    Terminal,
    
    /// <summary>
    /// Approach procedures
    /// </summary>
    Approach,
    
    /// <summary>
    /// Final approach segment
    /// </summary>
    Final,
    
    /// <summary>
    /// Missed approach procedure
    /// </summary>
    MissedApproach,
    
    /// <summary>
    /// Landing and rollout
    /// </summary>
    Landing,
    
    /// <summary>
    /// Holding pattern
    /// </summary>
    Holding,
    
    /// <summary>
    /// Emergency operations
    /// </summary>
    Emergency
}