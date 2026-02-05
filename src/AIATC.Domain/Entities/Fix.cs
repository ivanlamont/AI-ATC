using System;

namespace AIATC.Domain.Entities;

public class Fix
{
    public Guid Id { get; set; }
    public string FixIdentifier { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Type { get; set; }
    public Guid? AssociatedAirportId { get; set; }

    // Navigation properties
    public Airport? AssociatedAirport { get; set; }
}
