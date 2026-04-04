using AIATC.Data.Models.Types;
using AIATC.Data.Models.Procedures;
using AIATC.Data.Types.Common;

namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Airport and Heliport TAA</c> primary record.
</summary>
<remarks>See section 4.1.31.1 and 4.2.6.1.</remarks>*/
public class ArrivalAltitude : Record424
{
    public int Id { get; set; }

    public Port Port { get; set; }

    public Approach Approach { get; set; }

    public Fix Fix { get; set; }

    public FixPosition FixPosition { get; set; }

    public ArrivalSector[] Sectors { get; set; }

    public CourseType CourseType { get; set; }
}
