using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class Heliport
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? HeliportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? AtaIataDesignator { get; set; }

    public string? PadIdentifier { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? SpeedLimitAltitude { get; set; }

    public string? DatumCode { get; set; }

    public string? Ifr { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? MagneticVariation { get; set; }

    public string? Elevation { get; set; }

    public string? SpeedLimit { get; set; }

    public string? RecommendedVhfNavaid { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? TransitionAltitude { get; set; }

    public string? TransitionLevel { get; set; }

    public string? PublicMilitaryIndicator { get; set; }

    public string? TimeZone { get; set; }

    public string? DaylightIndicator { get; set; }

    public string? PadDimensions { get; set; }

    public string? MagneticTrueIndicator { get; set; }

    public string? HeliportName { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
