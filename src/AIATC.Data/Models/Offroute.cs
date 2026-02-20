using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
namespace AIATC.Data.Models;

/**<summary>
<c>Grid MORA</c> primary record.
</summary>
<remarks>See section 4.1.19.1.</remarks>*/
public class Offroute : Record424
{
    public int Id { get; set; }

    public Coordinates Coordinates { get; set; }

    public Altitude[] Altitudes { get; set; }
}
