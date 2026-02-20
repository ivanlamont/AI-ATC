using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
namespace AIATC.Data.Models.Tables;

/**<summary>
Fields of <c>Cruising Table</c>.
</summary>
<remarks>Used by <see cref="CruiseTable"/> like subsequence.</remarks>*/
public class CruiseColumn : Record424, ISequenced
{
    public int Id { get; set; }

    public int SeqNumber { get; set; }

    /**<summary>
    <c>Course FROM</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.135.</remarks>*/
    public float From { get; set; }

    /**<summary>
    <c>Course TO</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.135.</remarks>*/
    public float To { get; set; }

    /// <summary><c>Magnetic/True Indicator (M/T IND)</c> character.</summary>
    /// <remarks>See section 5.165.</remarks>
    public CourseType CourseType { get; set; }

    public Level[] Levels { get; set; }
}
