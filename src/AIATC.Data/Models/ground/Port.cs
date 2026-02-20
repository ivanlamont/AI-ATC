using AIATC.Data.Models.Types;
using AIATC.Data.Models.Procedures;
using AIATC.Data.Models.Ground;
using AIATC.Data.Models.Waypoints;
using AIATC.Data.Models.Navigation;
using AIATC.Data.Models.Comms;
using static AIATC.Data.Models.Types.Enums;

namespace AIATC.Data.Models.Ground;

// [DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Name)} - {{{nameof(Name)},nq}}")]
public abstract class Port : Fix, INamed
{
    /**<summary>
    <c>ATA/IATA Designator (ATA/IATA)</c> field.
    </summary>
    <remarks>See section 5.107.</remarks>*/
    public string? Designator { get; set; }

    /**<summary>
    <c>Speed Limit Altitude</c> field.
    </summary>
    <remarks>See section 5.73.</remarks>*/
    public Altitude Limit { get; set; }

    /**<summary>
    <c>IFR Capability (IFR)</c> character.
    </summary>
    <remarks>See section 5.108.</remarks>*/
    public Bool IsProcedurePublished { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /**<summary>
    <c>Airport/Heliport Elevation (ELEV)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.55.</remarks>*/
    public int Elevation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='SpeedLimit']/*"/>
    public int SpeedLimit { get; set; }

    /// <summary><c>Recommended NAVAID (RECD NAV)</c> field.</summary>
    public Omnidirect? Recommended { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Transition']/*"/>
    public int? TransitionAltitude { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Transition']/*"/>
    public int? TransitionLevel { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.Privacy"/>
    public Privacy Privacy { get; set; }

    /// <summary><c>Time Zone</c> field.</summary>
    /// <remarks>See section 5.178.</remarks>
    public string? TimeZone { get; set; }

    /**<summary>
    <c>Daylight Time Indicator (DAY TIME)</c> character.
    </summary>
    <remarks>See section 5.179.</remarks>*/
    public Bool IsDaylightTime { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.CourseType"/>
    public CourseType CourseType { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Datum']/*"/>
    public string? Datum { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Name']/*"/>
    public string? Name { get; set; }

    /// <summary>Associated GLSs.</summary>
    public GlobalLanding[]? GlobalLandings { get; set; }

    /// <summary>Associated MLSs.</summary>
    public MicrowaveLanding[]? MicrowaveLandings { get; set; }

    /// <summary>Associated ILSs.</summary>
    public InstrumentLanding[]? InstrumentLandings { get; set; }

    /// <summary>Associated ILS Markers.</summary>
    public InstrumentMarker[]? Markers { get; set; }

    /// <summary>Associated GBAS points.</summary>
    public GroundPoint[]? GroundPoints { get; set; }

    /// <summary>Associated SBAS points.</summary>
    public SatellitePoint[]? SatellitePoints { get; set; }

    /// <summary>Associated STARs.</summary>
    public Arrival[]? Arrivals { get; set; }

    /// <summary>Associated Approach Procedures.</summary>
    public Approach[]? Approaches { get; set; }

    /// <summary>Associated SIDs.</summary>
    public Departure[]? Departures { get; set; }

    /// <summary>Associated Communications.</summary>
    public PortCommunication[]? Communications { get; set; }

    /// <summary>Associated TAAs.</summary>
    public ArrivalAltitude[]? ArrivalAltitudes { get; set; }

    /// <summary>Associated MSAs.</summary>
    public MinimumAltitude[]? MinimumAltitudes { get; set; }

    /// <summary>Associated VHF Navaids.</summary>
    public Omnidirect[]? Omnidirects { get; set; }

    /// <summary>Associated NDBs.</summary>
    public TerminalBeacon[]? Beacons { get; set; }

    /// <summary>Associated Terminal Waypoints.</summary>
    public TerminalWaypoint[]? Waypoints { get; set; }

    /// <summary>Associated Helipads.</summary>
    public Pad[]? Pads { get; set; }
}
