using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;

namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Airport and Heliport MSA</c> primary record.
</summary>
<remarks>See section 4.1.20.1 and 4.2.4.</remarks>*/
public class MinimumAltitude : Record424, IMultiple
{
    public int Id { get; set; }

    public Port Port { get; set; }

    public IIdentity Center { get; set; }

    public char? Multiplier { get; set; }

    public Sector[] Sectors { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.CourseType"/>
    public CourseType CourseType { get; set; }
}
