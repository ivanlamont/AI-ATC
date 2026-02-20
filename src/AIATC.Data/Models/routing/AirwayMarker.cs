using AIATC.Data.Models.Types;
using System.Diagnostics;
using System.Diagnostics;
namespace AIATC.Data.Models.Routing;

/**<summary>
<c>Airways Marker</c> primary record.
</summary>
<remarks>See section 4.1.15.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public class AirwayMarker : Fix, INamed
{
    public int Id { get; set; }

    /**<summary>
    <c>Marker Code (MARKER CODE)</c> field.
    </summary>
    <remarks>See section 5.111.</remarks>*/
    public string MarkerCode { get; set; }

    /// <inheritdoc cref="MarkerShape"/>
    public MarkerShape Shape { get; set; }

    /// <inheritdoc cref="MarkerPower"/>
    public MarkerPower Power { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MinorAxis']/*"/>
    public float Bearing { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='FacElev']/*"/>
    public int Elevation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Datum']/*"/>
    public string? Datum { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Name']/*"/>
    public string? Name { get; set; }
}
