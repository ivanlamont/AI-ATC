using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using System.Diagnostics;
namespace AIATC.Data.Models.Routing;

/**<summary>
<c>Holding Pattern</c> primary record.
</summary>
<remarks>See section 4.1.5.1.</remarks>*/
[DebuggerDisplay($"{nameof(Fix)} - {{{nameof(Fix)}}}")]
public class HoldingPattern : Record424, IIcao, INamed
{
    public int Id { get; set; }

    public Icao Icao { get; set; }

    /// <summary><c>Duplicate Indicator (DUP IND)</c> field.</summary>
    /// <remarks>See section 5.114.</remarks>*/
    public string? DuplicateIndicator { get; set; }

    public Fix Fix { get; set; }

    /// <summary><c>Inbound Holding Course (IB HOLD CRS)</c> field.</summary>
    /// <remarks>See section 5.62.</remarks>*/
    public Course In { get; set; }

    /// <inheritdoc cref="Arinc424.Turn"/>
    public Turn Turn { get; set; }

    /**<summary>
    <c>Leg Length (LEG LENGTH)</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.64.</remarks>*/
    public float LegLength { get; set; }

    /**<summary>
    <c>Leg Time (LEG TIME)</c> field.
    </summary>
    <value>Minutes.</value>
    <remarks>See section 5.65.</remarks>*/
    public float LegTime { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Altitude']/*"/>
    public Altitude Minimum { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MaximumAltitude']/*"/>
    public Altitude Maximum { get; set; }

    /**<summary>
    <c>Holding Speed (HOLD SPEED)</c> field.
    </summary>
    <value>Knots.</value>
    <remarks>See section 5.175.</remarks>*/
    public int Speed { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RNP']/*"/>
    public float Performance { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='ArcRadius']/*"/>
    public float ArcRadius { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='VSF']/*"/>
    public int ScaleFactor { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RvsmMinimum']/*"/>
    public int MinLevel { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RvsmMaximum']/*"/>
    public int MaxLevel { get; set; }

    /// <inheritdoc cref="LegDirection"/>
    public LegDirection Direction { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Name']/*"/>
    public string? Name { get; set; }
}
