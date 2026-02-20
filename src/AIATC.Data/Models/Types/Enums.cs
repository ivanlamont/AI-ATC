namespace AIATC.Data.Models.Types;

public static class Enums
{
    // NAVIGATION ENUMS (moved from Arinc424.Navigation.Terms and Types)
    [System.Flags]
    public enum MarkerType : byte
    {
        Unknown = 0,
        Locator = 1,
        Inner = 1 << 1,
        Middle = 1 << 2,
        Outer = 1 << 3,
        Back = 1 << 4,
    }

        [System.Flags]
        public enum NondirectType : byte
        {
            Unknown = 0,
            Nondirect = 1,
            WithWeather = 1 << 1,
            Marine = 1 << 2,
            Inner = 1 << 3,
            Middle = 1 << 4,
            Outer = 1 << 5,
            Back = 1 << 6
        }

        public enum NondirectCoverage : byte
        {
            Unknown,
            HighPowered,
            Default,
            LowPowered,
            Locator
        }

        public enum NondirectInfo : byte
        {
            Unknown,
            AutomaticBroadcast,
            ScheduledBroadcast,
            NoVoice,
            Voice
        }

        public enum MarkerCollocation : byte
        {
            Unknown,
            BeatFrequencyOscillator,
            Collocated,
            Non
        }

        public enum OmnidirectCollocation : byte
        {
            Unknown,
            Collocated,
            Non
        }
    // COMMUNICATIONS ENUMS (moved from Arinc424.Comms.Terms)
    [Flags]
    public enum PortCommUsages : uint
    {
        Unknown = 0,
        Advisory = 1u,
        Community = 1u << 1,
        Departure = 1u << 2,
        FlightInfo = 1u << 3,
        Initial = 1u << 4,
        Arrival = 1u << 5,
        PreDepartureClearance = 1u << 6,
        // ... (add other values as needed)
    }

    [System.Flags]
    public enum AirwayCommUsages : ushort
    {
        Unknown = 0,
        AeronauticalInfo = 1,
        FlightInfo = 1 << 1,
        AirGround = 1 << 2,
        Discrete = 1 << 3,
        AirToAir = 1 << 4,
        Mandatory = 1 << 5,
        Secondary = 1 << 6,
        DirectionFinding = 1 << 7,
        RemoteAirToGround = 1 << 8,
        NonEnglish = 1 << 9,
        Military = 1 << 10,
        RemoteOutlet = 1 << 11
    }

    public enum DistanceLimitation : byte
    {
        Unknown,
        None,
        Out,
        Beyond
    }

    public enum Modulation : byte
    {
        Unknown,
        Amplitude,
        Frequency
    }

    public enum Emission : byte
    {
        Unknown,
        Double,
        SingleReducedCarrier,
        TwoIndependent,
        SingleFullCarrier,
        SingleSuppressedCarrier,
        LowerUnknownCarrier,
        UpperUnknownCarrier
    }

    public static class Terms
    {
        public string Value { get; set; } = string.Empty;
        
        // Nested types for backward compatibility with ARINC references
        public enum BoundaryVia { Unknown, Circle, GreatCircle, RhumbLine, Arc }
        public enum Arc { Unknown, Clockwise, CounterClockwise }
        public enum AirspaceClass { A, B, C, D, E, F, G, Unknown }
        public enum AirspaceType { Unknown, ControlZone, TerminalArea, ControlArea }
        public enum RestrictiveType { Unknown, Prohibited, Restricted, Danger, Warning, Alert, Military }
        public enum RegionType { FIR, UIR, CTA, TMA, Unknown }
        public enum SpeedReportUnit { Knots, KilometersPerHour }
        public enum AltitudeReportUnit { Feet, Meters, FlightLevel }
        public enum WaypointTypes { Unknown, Waypoint, NDB, VOR, Intersection }
        public enum WaypointUsages { Unknown, Terminal, Enroute, Both }
        public enum WaypointNameFormats { Unknown, FiveLetterName, LatLon }
        public enum ApproachTypes { Unknown, ILS, VOR, NDB, GPS, RNAV, Localizer, Visual }
    }

    public class Bool
    {
        public bool Value { get; set; }
    }

    public class Icao
    {
        public string Code { get; set; } = string.Empty;
    }

    public enum CourseType
    {
        True,
        Magnetic
    }

    public enum CommType
    {
        Unknown,
        Approach,
        Departure,
        Tower,
        Ground,
        Center,
        Clearance,
        Unicom,
        Multicom,
        ATIS,
        FlightService
    }

    public enum Turn
    {
        Left,
        Right,
        Either
    }

    public enum LevelType
    {
        AGL,
        MSL,
        FlightLevel
    }

    public enum TimeCode
    {
        Unknown,
        Sunrise,
        Sunset,
        Continuous
    }

    public enum LimitUnit
    {
        Feet,
        Meters,
        FlightLevel
    }

    public enum AltitudeDescription
    {
        AtOrAbove,
        AtOrBelow,
        At,
        Between,
        Unspecified
    }

    public enum LegDirection
    {
        Inbound,
        Outbound,
        Either
    }

    public enum Privacy
    {
        Public,
        Private,
        Restricted
    }

    public enum Sectorization
    {
        Unknown,
        Defined
    }
}

// Support legacy ARINC namespace references
namespace AIATC.Data.Models.Types.Airspace
{
    public static class Terms
    {
        public enum RegionType
        {
            FIR,
            UIR,
            CTA,
            TMA,
            Unknown
        }
    }
}

namespace AIATC.Data.Models.Types.Waypoints
{
    public static class Terms
    {
        public enum WaypointDescriptions
        {
            Unnamed,
            NDB,
            OffRoute,
            Runway,
            VFRReportingPoint,
            Unknown
        }
    }
}
