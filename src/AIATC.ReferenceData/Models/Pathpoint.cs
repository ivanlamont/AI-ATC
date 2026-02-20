using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class Pathpoint
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? ApproachIdentifier { get; set; }

    public string? RunwayOrHelipadIdentifier { get; set; }

    public string? OperationsType { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? RouteIndicator { get; set; }

    public string? SbasServiceProvider { get; set; }

    public string? ReferencePathDataSelector { get; set; }

    public string? ReferencePathDataIdentifier { get; set; }

    public string? ApproachPerformanceDesignator { get; set; }

    public string? LtpLatitude { get; set; }

    public string? LtpLongitude { get; set; }

    public string? LtpEllipsoidHeight { get; set; }

    public string? Gpa { get; set; }

    public string? FpapLatitude { get; set; }

    public string? FpapLongitude { get; set; }

    public string? CourseWidthAtThreshold { get; set; }

    public string? LengthOffset { get; set; }

    public string? Tch { get; set; }

    public string? TchUnitsSelector { get; set; }

    public string? Hal { get; set; }

    public string? Val { get; set; }

    public string? CrcRemainder { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
