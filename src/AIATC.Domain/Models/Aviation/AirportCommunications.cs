namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Communication frequencies for an airport
/// </summary>
public class AirportCommunications
{
    /// <summary>
    /// Air Traffic Control Tower frequency
    /// </summary>
    public float? TowerFrequency { get; set; }

    /// <summary>
    /// Ground Control frequency
    /// </summary>
    public float? GroundFrequency { get; set; }

    /// <summary>
    /// Approach Control frequency
    /// </summary>
    public float? ApproachFrequency { get; set; }

    /// <summary>
    /// Departure Control frequency
    /// </summary>
    public float? DepartureFrequency { get; set; }

    /// <summary>
    /// ATIS (Automatic Terminal Information Service) frequency
    /// </summary>
    public float? AtisFrequency { get; set; }

    /// <summary>
    /// Clearance Delivery frequency
    /// </summary>
    public float? ClearanceDeliveryFrequency { get; set; }

    /// <summary>
    /// Unicom frequency for uncontrolled airports
    /// </summary>
    public float? UnicomFrequency { get; set; }
}