using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class ControlledAirspace
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? IcaoCode { get; set; }

    public string? AirspaceType { get; set; }

    public string? AirspaceCenter { get; set; }

    public string? SectionCode2 { get; set; }

    public string? SubsectionCode2 { get; set; }

    public string? AirspaceClass { get; set; }

    public string? MultipleCode { get; set; }

    public string? SequenceNumber { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? Level { get; set; }

    public string? TimeCode { get; set; }

    public string? Notam { get; set; }

    public string? BoundaryVia { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? ArcOriginLatitude { get; set; }

    public string? ArcOriginLongitude { get; set; }

    public string? ArcDistance { get; set; }

    public string? ArcBearing { get; set; }

    public string? Rnp { get; set; }

    public string? LowerLimit { get; set; }

    public string? LowerLimitUnitIndicator { get; set; }

    public string? UpperLimit { get; set; }

    public string? UpperLimitUnitIndicator { get; set; }

    public string? AirspaceName { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
