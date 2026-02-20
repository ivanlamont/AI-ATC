using System.Diagnostics;
using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Airspace;

/**<summary>
Multiple <c>FIR/UIR</c> primary record sequences.
</summary>
<remarks>See section 4.1.17.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {{{nameof(Name)},nq}}")]
public class FlightRegion : Record424<RegionVolume>, IIdentity, INamed
{
    public int Id { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='FIR']/*"/>
    public string Identifier { get; set; }

    /**<summary>
    <c>FIR/UIR Address (ADDRESS)</c> field.
    </summary>
    <remarks>See section 5.151.</remarks>*/
    public string Address { get; set; }

    /**<summary>
    <c>FIR/UIR Name</c> field.
    </summary>
    <remarks>See section 5.125.</remarks>*/
    public string? Name { get; set; }
}
