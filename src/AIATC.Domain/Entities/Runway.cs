using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class Runway
{
    public Guid Id { get; set; }
    public Guid AirportId { get; set; }
    public string RunwayIdentifier { get; set; } = string.Empty;
    public int MagneticHeading { get; set; }
    public int LengthFt { get; set; }
    public int WidthFt { get; set; }
    public string? SurfaceType { get; set; }
    public bool HasIls { get; set; }
    public decimal? LocalizerFrequency { get; set; }
    public decimal GlideslopeAngle { get; set; } = 3.0m;
    public int DisplacedThresholdFt { get; set; }
    public int ElevationFt { get; set; }
    public decimal? LatitudeThreshold { get; set; }
    public decimal? LongitudeThreshold { get; set; }

    // Navigation properties
    public Airport Airport { get; set; } = null!;
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
}
