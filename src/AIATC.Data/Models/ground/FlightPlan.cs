namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Flight Planning</c> primary record.
</summary>
<remarks>See section 4.1.27.1.</remarks>*/
[Obsolete("todo")]
public class FlightPlan : Record424
{
    public int Id { get; set; }

    public string AirportIdentifier { get; set; }

    public string IcaoCode { get; set; }

    public string ProcedureIdentifier { get; set; }

    public char ProcedureType { get; set; }

    public string RunwayTransitionIdentifier { get; set; }

    public string RunwayTransitionFix { get; set; }

    public string RunwayTransitionFixIcaoCode { get; set; }

    public char RunwayTransitionFixSectionCode { get; set; }

    public char FixSubsectionCode { get; set; }

    public string RunwayTransitionAlongTrackDistance { get; set; }

    public string CommonSegmentTransitionFix { get; set; }

    public string CommonSegmentTransitionFixIcaoCode { get; set; }

    public char CommonSegmentTransitionFixSectionCode { get; set; }

    public char CommonSegmentTransitionFixSubsectionCode { get; set; }

    public string CommonSegmentAlongTrackDistance { get; set; }

    public string EnrouteTransitionIdentifier { get; set; }

    public string EnrouteTransitionFix { get; set; }

    public string EnrouteTransitionFixIcaoCode { get; set; }

    public char EnrouteTransitionFixSectionCode { get; set; }

    public char EnrouteTransitionFixSubsectionCode { get; set; }

    public string EnrouteTransitionAlongTrackDistance { get; set; }

    public string SequenceNumber { get; set; }

    public char ContinuationRecordNumber { get; set; }

    public string EnginesNumber { get; set; }

    /// <summary>
    /// <c>Turboprop/Jet Indicator (TURBO)</c> character.
    /// </summary>
    /// <remarks>See section 5.233.</remarks>
    public char EngineTypeRestriction { get; set; }

    public char IsRnav { get; set; }

    public char AtcWeightCategory { get; set; }

    public string AtcIdentifier { get; set; }

    public char TimeCode { get; set; }

    public string ProcedureDescription { get; set; }

    public string LegTypeCode { get; set; }

    public char ReportingCode { get; set; }

    public string InitialDepartureMagneticCourse { get; set; }

    public char AltitudeDescription { get; set; }

    public string FirstAltitude { get; set; }

    public string SecondAltitude { get; set; }

    public string SpeedLimit { get; set; }

    public string InitialCruiseTable { get; set; }

    public char SpeedLimitDescription { get; set; }
}
