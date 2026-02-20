using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class TerminalNavaid
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? NdbIdentifier { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? NdbFrequency { get; set; }

    public string? NdbClass { get; set; }

    public string? NdbLatitude { get; set; }

    public string? NdbLongitude { get; set; }

    public string? MagneticVariation { get; set; }

    public string? DatumCode { get; set; }

    public string? NdbName { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
