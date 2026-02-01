using System;

namespace AIATC.Domain.Entities;

public class Weather
{
    public Guid Id { get; set; }
    public Guid AirportId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? WindDirectionDeg { get; set; }
    public int? WindSpeedKts { get; set; }
    public int? WindGustKts { get; set; }
    public decimal? VisibilitySm { get; set; }
    public int? CeilingFt { get; set; }
    public int? TemperatureC { get; set; }
    public int? DewpointC { get; set; }
    public decimal? AltimeterInHg { get; set; }
    public string[]? WeatherPhenomena { get; set; }
    public string? MetarRaw { get; set; }
    public string? TafRaw { get; set; }
    public string? Source { get; set; }
    public DateTime FetchedAt { get; set; }

    // Navigation properties
    public Airport Airport { get; set; } = null!;
}
