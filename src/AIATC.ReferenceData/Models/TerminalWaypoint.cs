using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class TerminalWaypoint
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? WaypointIdentifier { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? Type { get; set; }

    public string? Usage { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? DynamicMagneticVariation { get; set; }

    public string? Elevation { get; set; }

    public string? DatumCode { get; set; }

    public string? NameFormatIndicator { get; set; }

    public string? NameDescription { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
