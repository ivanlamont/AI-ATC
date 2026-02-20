
using AIATC.Data.Models.Types;
using System.Diagnostics;
namespace AIATC.Data.Models.Airspace;

/**<summary>
Fields of <c>Controlled Airspace</c> and <c>Restrictive Airspace</c>.
</summary>
<remarks>Used by <see cref="ControlledVolume"/> and <see cref="RestrictiveVolume"/> like subsequence.</remarks>*/
// ...existing code...
public class BoundaryPoint : Geo, ISequenced
{
    public int Id { get; set; }
    public int SeqNumber { get; set; }

    // Removed ARINC/type references and attributes
}
