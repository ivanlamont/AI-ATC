using System.Collections.Generic;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Aviation;

namespace AIATC.Web.Services;

/// <summary>
/// Service for providing airport and runway data
/// In a production system, this would load from an ARINC database or similar aviation data source
/// </summary>
public interface IAirportDataService
{
    /// <summary>
    /// Gets an airport by ICAO code
    /// </summary>
    AirportModel? GetAirport(string icaoCode);

    /// <summary>
    /// Gets all runways for an airport
    /// </summary>
    List<Runway> GetRunways(string airportIcaoCode);

    /// <summary>
    /// Gets all airports in the database
    /// </summary>
    List<AirportModel> GetAllAirports();
}

/// <summary>
/// Implementation of airport data service with sample data
/// </summary>
public class AirportDataService : IAirportDataService
{
    private readonly List<AirportModel> _airports;
    private readonly List<Runway> _runways;

    public AirportDataService()
    {
        // Initialize with sample airport data
        _airports = new List<AirportModel>
        {
            new AirportModel
            {
                IcaoCode = "KSFO",
                PositionNm = new Vector2(0, 0),
                AltitudeFt = 13,
                Name = "San Francisco International Airport"
            },
            new AirportModel
            {
                IcaoCode = "KATL",
                PositionNm = new Vector2(0, 0),
                AltitudeFt = 1026,
                Name = "Hartsfield-Jackson Atlanta International Airport"
            },
            new AirportModel
            {
                IcaoCode = "KLAX",
                PositionNm = new Vector2(0, 0),
                AltitudeFt = 126,
                Name = "Los Angeles International Airport"
            }
        };

        // Initialize with sample runway data
        _runways = new List<Runway>
        {
            // KSFO Runways
            new Runway
            {
                Identifier = "10L/28R",
                AirportIcaoCode = "KSFO",
                MagneticBearing = 100,
                LengthFt = 11870,
                WidthFt = 200,
                ThresholdPositionNm = new Vector2(-2, -2),
                EndPositionNm = new Vector2(2, 2),
                ThresholdElevationFt = 13,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true,
                LocalizerFrequency = 111.15f,
                LocalizerCourse = 103.0f
            },
            new Runway
            {
                Identifier = "10R/28L",
                AirportIcaoCode = "KSFO",
                MagneticBearing = 100,
                LengthFt = 8650,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-1.5f, -1.5f),
                EndPositionNm = new Vector2(1.5f, 1.5f),
                ThresholdElevationFt = 13,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true,
                LocalizerFrequency = 111.05f,
                LocalizerCourse = 103.0f
            },
            new Runway
            {
                Identifier = "01L/19R",
                AirportIcaoCode = "KSFO",
                MagneticBearing = 10,
                LengthFt = 6800,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-1, 1),
                EndPositionNm = new Vector2(1, -1),
                ThresholdElevationFt = 13,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = false
            },
            new Runway
            {
                Identifier = "01R/19L",
                AirportIcaoCode = "KSFO",
                MagneticBearing = 10,
                LengthFt = 6800,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-0.8f, 0.8f),
                EndPositionNm = new Vector2(0.8f, -0.8f),
                ThresholdElevationFt = 13,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = false
            },

            // KATL Runways
            new Runway
            {
                Identifier = "08L/26R",
                AirportIcaoCode = "KATL",
                MagneticBearing = 80,
                LengthFt = 10000,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2.5f, -2.5f),
                EndPositionNm = new Vector2(2.5f, 2.5f),
                ThresholdElevationFt = 1026,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },
            new Runway
            {
                Identifier = "08R/26L",
                AirportIcaoCode = "KATL",
                MagneticBearing = 80,
                LengthFt = 10000,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2.2f, -2.2f),
                EndPositionNm = new Vector2(2.2f, 2.2f),
                ThresholdElevationFt = 1026,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },
            new Runway
            {
                Identifier = "09/27",
                AirportIcaoCode = "KATL",
                MagneticBearing = 90,
                LengthFt = 9000,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2, 0),
                EndPositionNm = new Vector2(2, 0),
                ThresholdElevationFt = 1026,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },

            // KLAX Runways
            new Runway
            {
                Identifier = "06L/24R",
                AirportIcaoCode = "KLAX",
                MagneticBearing = 60,
                LengthFt = 10886,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-3, -1),
                EndPositionNm = new Vector2(3, 1),
                ThresholdElevationFt = 126,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },
            new Runway
            {
                Identifier = "06R/24L",
                AirportIcaoCode = "KLAX",
                MagneticBearing = 60,
                LengthFt = 10886,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2.8f, -0.8f),
                EndPositionNm = new Vector2(2.8f, 0.8f),
                ThresholdElevationFt = 126,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },
            new Runway
            {
                Identifier = "07L/25R",
                AirportIcaoCode = "KLAX",
                MagneticBearing = 70,
                LengthFt = 8925,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2.5f, 0.5f),
                EndPositionNm = new Vector2(2.5f, -0.5f),
                ThresholdElevationFt = 126,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            },
            new Runway
            {
                Identifier = "07R/25L",
                AirportIcaoCode = "KLAX",
                MagneticBearing = 70,
                LengthFt = 8925,
                WidthFt = 150,
                ThresholdPositionNm = new Vector2(-2.3f, 0.7f),
                EndPositionNm = new Vector2(2.3f, -0.7f),
                ThresholdElevationFt = 126,
                Surface = RunwaySurface.Concrete,
                HasPrecisionApproach = true
            }
        };
    }

    public AirportModel? GetAirport(string icaoCode)
    {
        return _airports.Find(a => a.IcaoCode.Equals(icaoCode, System.StringComparison.OrdinalIgnoreCase));
    }

    public List<Runway> GetRunways(string airportIcaoCode)
    {
        return _runways.FindAll(r => r.AirportIcaoCode.Equals(airportIcaoCode, System.StringComparison.OrdinalIgnoreCase));
    }

    public List<AirportModel> GetAllAirports()
    {
        return new List<AirportModel>(_airports);
    }
}