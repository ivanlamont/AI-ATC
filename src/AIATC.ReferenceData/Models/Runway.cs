using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class Runway
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? RunwayIdentifier { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? RunwayLength { get; set; }

    public string? RunwayBearing { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? RunwayGradient { get; set; }

    public string? LtpElipsoidHeight { get; set; }

    public string? LandingThresholdElevation { get; set; }

    public string? DisplacedThreshold { get; set; }

    public string? Tch { get; set; }

    public string? Width { get; set; }

    public string? TchValueIndicator { get; set; }

    public string? LocMlsGlsIdentifier { get; set; }

    public string? CategoryClass { get; set; }

    public string? Stopway { get; set; }

    public string? SecondaryLocMlsGlsIdentifier { get; set; }

    public string? CategoryClass2 { get; set; }

    public string? RunwayDescription { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
