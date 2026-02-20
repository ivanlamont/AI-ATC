using AIATC.Data.Models.Types;
using System.Diagnostics;
namespace AIATC.Data.Models.Comms;
using AIATC.Data.Models.Types;

using AIATC.Data.Models.Types;

/**<summary>
Fields of <c>Enroute Communications</c>.
</summary>
<remarks>Used by <see cref="AirwayCommunication"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{{{nameof(Narrative)},nq}}, {nameof(Type)} - {{{nameof(Type)}}}")]
public class AirwayTransmitter : Transmitter
{
    public int Id { get; set; }

    /**<summary>
    <c>Sectorization Narrative</c> field.
    </summary>
    <remarks>See section 5.186.
    <c>Remote Name</c> field before supplement 19, see section 5.189.
    </remarks>*/
    public string? Narrative { get; set; }

    /// <inheritdoc cref="AirwayCommUsages"/>
    public AirwayCommUsages Usages { get; set; }
}
