
using AIATC.Data.Models.Types;
using System.Diagnostics;
using System.Diagnostics;
using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Waypoints;

/**<summary>
<c>Waypoint</c> primary record.
</summary>
<remarks>See section 4.1.4.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public class Waypoint : Fix, INamed
{
    public int Id { get; set; }

    public WaypointTypes Types { get; set; }

    public WaypointUsages Usages { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Datum']/*"/>
    public string? Datum { get; set; }

    public WaypointNameFormats NameFormats { get; set; }

    /// <summary><c>Waypoint Name (NAME)</c> field.</summary>
    /// <remarks>See section 5.43.</remarks>
    public string? Name { get; set; }
}
