using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class VhfNavaid
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? VorIdentifier { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? VorFrequency { get; set; }

    public string? NavaidClass { get; set; }

    public string? VorLatitude { get; set; }

    public string? VorLongitude { get; set; }

    public string? DmeIdentifier { get; set; }

    public string? DmeLatitude { get; set; }

    public string? DmeLongitude { get; set; }

    public string? StationDeclination { get; set; }

    public string? DmeElevation { get; set; }

    public string? Fom { get; set; }

    public string? IlsDmeBias { get; set; }

    public string? FrequencyProtection { get; set; }

    public string? DatumCode { get; set; }

    public string? VorName { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
