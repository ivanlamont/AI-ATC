using System.Diagnostics;
using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Airspace;

/**<summary>
<c>Controlled Airspace</c> primary record sequence.
</summary>
<remarks>Used by <see cref="ControlledSpace"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{nameof(Class)} - {{{nameof(Class)},nq}}, {nameof(Type)} - {{{nameof(Type)}}}")]
public class ControlledVolume : Volume
{
    public int Id { get; set; }

    /// <inheritdoc cref="AirspaceType"/>
    public AirspaceType Type { get; set; }

    /**<summary>
    <c>Controlled Airspace Center (ARSP CNTR)</c> field.
    </summary>
    <remarks>See section 5.214.</remarks>*/
    public IIdentity Center { get; set; }

    /// <inheritdoc cref="AirspaceClass"/>
    public AirspaceClass Class { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='RNP']/*"/>
    public float Performance { get; set; }
}
