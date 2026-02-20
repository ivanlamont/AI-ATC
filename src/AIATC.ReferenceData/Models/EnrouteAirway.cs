using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class EnrouteAirway
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? SubsectionCode { get; set; }

    public string? RouteIdentifier { get; set; }

    public string? SixthCharacter { get; set; }

    public string? SequenceNumber { get; set; }

    public string? FixIdentifier { get; set; }

    public string? IcaoCode { get; set; }

    public string? SectionCode2 { get; set; }

    public string? SubsectionCode2 { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? WaypointDescriptionCode { get; set; }

    public string? BoundaryCode { get; set; }

    public string? RouteType { get; set; }

    public string? Level { get; set; }

    public string? DirectionRestriction { get; set; }

    public string? CruiseTableIndicator { get; set; }

    public string? EuIndicator { get; set; }

    public string? RecommendedVhfNavaid { get; set; }

    public string? IcaoCode2 { get; set; }

    public string? Rnp { get; set; }

    public string? Theta { get; set; }

    public string? Rho { get; set; }

    public string? OutboundMagneticCourse { get; set; }

    public string? RouteDistanceFrom { get; set; }

    public string? InboundMagneticCourse { get; set; }

    public string? MinimumAltitude1 { get; set; }

    public string? MinimumAltitude2 { get; set; }

    public string? MaximumAltitude { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
