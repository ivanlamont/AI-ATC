using System;

namespace AIATC.Domain.Entities;

public class Procedure
{
    public Guid Id { get; set; }
    public Guid AirportId { get; set; }
    public Guid? RunwayId { get; set; }
    public string ProcedureType { get; set; } = string.Empty;  // SID, STAR, IAP
    public string ProcedureName { get; set; } = string.Empty;
    public string? ProcedureIdentifier { get; set; }
    public string Waypoints { get; set; } = string.Empty;  // JSON string
    public int? MinimumAltitudeFt { get; set; }
    public string? WeatherMinimums { get; set; }  // JSON string
    public string? Notes { get; set; }

    // Navigation properties
    public Airport Airport { get; set; } = null!;
    public Runway? Runway { get; set; }
}
