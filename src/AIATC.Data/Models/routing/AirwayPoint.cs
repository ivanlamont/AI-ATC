using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using System.Diagnostics;
namespace AIATC.Data.Models.Routing;

/**<summary>
Fields of <c>Enroute Airways</c> primary record.
</summary>
<remarks>Used by <see cref="Airway"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{nameof(Fix)} - {{{nameof(Fix)}}}")]
public class AirwayPoint : Record424, ISequenced
{
    public int Id { get; set; }

    public int SeqNumber { get; set; }

    public Fix Fix { get; set; }

    public WaypointDescriptions Descriptions { get; set; }

    public BoundaryCode BoundaryCode { get; set; }

    public AirwayType Type { get; set; }

    public LevelType LevelType { get; set; }

    public AirwayRestriction Restriction { get; set; }

    /// <inheritdoc cref="Tables.CruiseTable"/>
    public Tables.CruiseTable? CruiseTable { get; set; }

    /// <summary><c>EU Indicator (EU IND)</c> character.</summary>
    /// <remarks>See section 5.164.</remarks>
    public Bool HasRestrictions { get; set; }

    public Navigation.Omnidirect? Recommended { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RNP']/*"/>
    public float Performance { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Theta']/*"/>
    public float Theta { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Rho']/*"/>
    public float Rho { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='OutboundMagneticCourse']/*"/>
    public Course Out { get; set; }

    /**<summary>
    <c>Route Distance From (RTE DIST FROM)</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.27.</remarks>*/
    public float DistanceFrom { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='InboundMagneticCourse']/*"/>
    public Course In { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Altitude']/*"/>
    public Altitude Minimum { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Altitude']/*"/>
    public Altitude Minimum2 { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MaximumAltitude']/*"/>
    public Altitude Maximum { get; set; }

    /**<summary>
    <c>Fixed Radius Transition Indicator (FIXED RAD IND)</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.254</remarks>*/
    public float FixRadius { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='VSF']/*"/>
    public int ScaleFactor { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RvsmMinimum']/*"/>
    public int MinLevel { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RvsmMaximum']/*"/>
    public int MaxLevel { get; set; }

    public AirwayContinuation[]? Notes { get; set; }
}
