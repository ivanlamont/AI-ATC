using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class Airport
{
    public Guid Id { get; set; }
    public string IcaoCode { get; set; } = string.Empty;
    public string? IataCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int ElevationFt { get; set; }
    public string? Timezone { get; set; }
    public string? CountryCode { get; set; }

    // Navigation properties
    public ICollection<Runway> Runways { get; set; } = new List<Runway>();
    public ICollection<Fix> Fixes { get; set; } = new List<Fix>();
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    public ICollection<Weather> WeatherRecords { get; set; } = new List<Weather>();
}
