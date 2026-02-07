using Microsoft.EntityFrameworkCore;
using AIATC.Domain.Models.Aviation;

namespace AIATC.Data.Seeding;

/// <summary>
/// Service to seed aviation database with sample data
/// </summary>
public class AviationDataSeeder
{
    private readonly AviationDbContext _context;

    public AviationDataSeeder(AviationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds the database with sample aviation data if it's empty
    /// </summary>
    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await _context.AircraftTypes.AnyAsync())
        {
            return; // Database already seeded
        }

        await SeedAircraftTypesAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedAircraftTypesAsync()
    {
        var aircraftTypes = new[]
        {
            new AircraftType
            {
                IcaoCode = "B738",
                Name = "Boeing 737-800",
                Category = AircraftCategory.C,
                WakeCategory = WakeCategory.Medium,
                MaxTakeoffWeightLbs = 174200,
                ServiceCeilingFt = 41000,
                VrefSpeedKnots = 130,
                MinApproachSpeedKnots = 120,
                MaxCruiseSpeedKnots = 250,
                TypicalCruiseSpeedKnots = 220,
                MaxClimbRateFpm = 3000,
                MaxDescentRateFpm = 3500
            },
            new AircraftType
            {
                IcaoCode = "A320",
                Name = "Airbus A320",
                Category = AircraftCategory.C,
                WakeCategory = WakeCategory.Medium,
                MaxTakeoffWeightLbs = 172000,
                ServiceCeilingFt = 39800,
                VrefSpeedKnots = 135,
                MinApproachSpeedKnots = 125,
                MaxCruiseSpeedKnots = 250,
                TypicalCruiseSpeedKnots = 230,
                MaxClimbRateFpm = 2800,
                MaxDescentRateFpm = 3200
            },
            new AircraftType
            {
                IcaoCode = "C172",
                Name = "Cessna 172",
                Category = AircraftCategory.A,
                WakeCategory = WakeCategory.Light,
                MaxTakeoffWeightLbs = 2550,
                ServiceCeilingFt = 14000,
                VrefSpeedKnots = 60,
                MinApproachSpeedKnots = 55,
                MaxCruiseSpeedKnots = 120,
                TypicalCruiseSpeedKnots = 100,
                MaxClimbRateFpm = 700,
                MaxDescentRateFpm = 1000
            }
        };

        _context.AircraftTypes.AddRange(aircraftTypes);
    }
}