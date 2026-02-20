using System.Diagnostics;
using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Airspace;

/**<summary>
Multiple <c>Controlled Airspace</c> primary record sequences.
</summary>
<remarks>See section 4.1.25.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Icao)},nq}}, {{{nameof(Name)},nq}}")]
public class ControlledSpace : Space<ControlledVolume>
{
    public int Id { get; set; }
}

/**<summary>
Multiple <c>Restrictive Airspace</c> primary record sequences.
</summary>
<remarks>See section 4.1.18.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Icao)},nq}}, {{{nameof(Identifier)},nq}}")]
public class RestrictiveSpace : Space<RestrictiveVolume>, IIdentity
{
    public int Id { get; set; }

    /**<summary>
    <c>Restrictive Airspace Designation</c> field.
    </summary>
    <remarks>See section 5.129.</remarks>*/
    public string Identifier { get; set; }
}
