using System;
using System.Collections.Generic;

namespace AIATC.ReferenceData.Models;

public partial class Localizer
{
    public string? RecordType { get; set; }

    public string? CustomerAreaCode { get; set; }

    public string? SectionCode { get; set; }

    public string? AirportIdentifier { get; set; }

    public string? SubsectionCode { get; set; }

    public string? LocalizerIdentifier { get; set; }

    public string? IlsCategory { get; set; }

    public string? ContinuationRecord { get; set; }

    public string? LocalizerFrequency { get; set; }

    public string? RunwayIdentifier { get; set; }

    public string? LocalizerLatitude { get; set; }

    public string? LocalizerLongitude { get; set; }

    public string? LocalizerBearing { get; set; }

    public string? GlideSlopeLatitude { get; set; }

    public string? GlideSlopeLongitude { get; set; }

    public string? LocalizerPosition { get; set; }

    public string? LocalizerPositionRef { get; set; }

    public string? GlideSlopePosition { get; set; }

    public string? LocalizerWidth { get; set; }

    public string? GlideSlopeAngle { get; set; }

    public string? StationDeclination { get; set; }

    public string? GsThresholdLandingHeight { get; set; }

    public string? GlideSlopeElevation { get; set; }

    public string? FileRecordNumber { get; set; }

    public string? Cycle { get; set; }
}
