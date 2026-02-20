using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class PathpointCont
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

    public string? ApplicationType { get; set; }

    public string? FpapEllipsoidHeight { get; set; }

    public string? FpapOrthometricHeight { get; set; }

    public string? LtpOrthometricHeight { get; set; }

    public string? ApproachTypeIdentifier { get; set; }

    public string? GnssChannelNumber { get; set; }

    public string? Hpc { get; set; }

    public string? Reserved { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
