using System.Diagnostics;
using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Airspace;

/**<summary>
<c>Restrictive Airspace</c> primary record sequence.
</summary>
<remarks>Used by <see cref="RestrictiveSpace"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{{{nameof(Type)},nq}}")]
public class RestrictiveVolume : Volume
{
    public int Id { get; set; }

    /// <inheritdoc cref="RestrictiveType"/>
    public RestrictiveType Type { get; set; }
}
