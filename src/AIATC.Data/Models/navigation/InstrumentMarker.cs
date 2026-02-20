namespace AIATC.Data.Models.Navigation;

using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using AIATC.Data.Models.Ground;

/**<summary>
<c>Airport and Heliport Localizer Marker</c> primary record.
</summary>
<remarks>See section 4.1.13.1.</remarks>*/
// [DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class InstrumentMarker : Fix
{
    public int Id { get; set; }

    public Port Port { get; set; }

    public InstrumentLanding Landing { get; set; }

    public Touch Touch { get; set; }

    public MarkerType Type { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Frequency']/*"/>
    public float Frequency { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MinorAxis']/*"/>
    public float Bearing { get; set; }

    public Coordinates LocatorCoordinates { get; set; }

    public NondirectType NavaidType { get; set; }
    public NondirectCoverage Coverage { get; set; }
    public NondirectInfo Info { get; set; }
    public MarkerCollocation Collocation { get; set; }

    [Obsolete("need more section 5.93 analysis")]
    public string? Facility { get; set; }

    /**<summary>
    <c>VOR/NDB Identifier (VOR IDENT/NDB IDENT)</c> field.
    </summary>
    <remarks>See section 5.33.</remarks>*/
    public string? LocatorIdentifier { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='FacElev']/*"/>
    public int Elevation { get; set; }
}
