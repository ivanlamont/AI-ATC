using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using System.Diagnostics;
namespace AIATC.Data.Models.Procedures;

/**<summary>
Fields of <c>SID/STAR/Approach</c>.
</summary>*/
[DebuggerDisplay($"{nameof(Fix)} - {{{nameof(Fix)}}}")]
public abstract class ProcedurePoint : Record424, ISequenced
{
    public int SeqNumber { get; set; }

    public Fix? Fix { get; set; }

    /// <inheritdoc cref="WaypointDescriptions"/>
    [Obsolete("maybe split to 4 enums for SID/STAR/Approach and Airway?")]
    public WaypointDescriptions Descriptions { get; set; }

    /// <inheritdoc cref="Arinc424.Turn"/>
    public Turn Turn { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RNP']/*"/>
    public float Performance { get; set; }

    /// <inheritdoc cref="LegType"/>
    public LegType LegType { get; set; }

    /// <summary><c>Turn Direction Valid (TDV)</c> character.</summary>
    /// <remarks>See section 5.22.</remarks>
    public Bool IsTurnRequired { get; set; }

    public Fix? Recommended { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='ArcRadius']/*"/>
    public float ArcRadius { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Theta']/*"/>
    public float Theta { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Rho']/*"/>
    public float Rho { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='OutboundMagneticCourse']/*"/>
    public Course Course { get; set; }

    /**<summary>
    <c>Route Distance From, Holding Distance/Time (RTE DIST FROM, HOLD DIST/TIME)</c> field.
    </summary>
    <remarks>See section 5.27.</remarks>*/
    public string? DistanceTiming { get; set; }

    /// <inheritdoc cref="LegDirection"/>
    public LegDirection Direction { get; set; }

    /// <inheritdoc cref="Arinc424.AltitudeDescription"/>
    public AltitudeDescription AltitudeDescription { get; set; }

    /**<summary>
    <c>ATC Indicator (ATC)</c> character.
    </summary>
    <remarks>See section 5.81.</remarks>*/
    public Bool IsAltitudeModifiable { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Altitude']/*"/>
    public Altitude Altitude { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Altitude']/*"/>
    public Altitude Altitude2 { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='TransitionAltitude']/*"/>
    public int TransitionAltitude { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='SpeedLimit']/*"/>
    public int SpeedLimit { get; set; }

    /**<summary>
    <c>Center Fix (CENTER FIX)</c> or <c>TAA Sector Identifier</c> field.
    </summary>
    <remarks>See section 5.144 or 5.272.</remarks>*/
    public Fix? Center { get; set; }

    /**<summary>
    <c>Multiple Code (MULTI CD)</c> or <c>Procedure Turn Indicator</c> character.
    </summary>
    <remarks>See section 5.130 or 5.271.</remarks>*/
    public char MultiplierOrTurn { get; set; }

    /// <inheritdoc cref="Overlay"/>
    public Overlay Overlay { get; set; }

    /// <inheritdoc cref="SpeedLimitType"/>
    public SpeedLimitType SpeedLimitType { get; set; }
}
