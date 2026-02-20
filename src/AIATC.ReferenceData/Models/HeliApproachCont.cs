using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class HeliApproachCont
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? HeliportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? ProcedureIdentifier { get; set; }

    public string? RouteType { get; set; }

    public string? TransitionIdentifier { get; set; }

    public string? SequenceNumber { get; set; }

    public string? FixIdentifier { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? SectionCode2 { get; set; }

    public string? SubsectionCode2 { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? ApplicationType { get; set; }

    public string? FasBlock { get; set; }

    public string? FasBlockLosName { get; set; }

    public string? LnavVnav { get; set; }

    public string? LnavVnavLosName { get; set; }

    public string? Lnav { get; set; }

    public string? LnavLosName { get; set; }

    public string? RouteQualifier1 { get; set; }

    public string? RouteQualifier2 { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
