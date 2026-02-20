using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Procedures;

/**<summary>
Fields of <c>Airport</c> and <c>Heliport Approach</c>.
</summary>
<remarks>Used by <see cref="ApproachSequence"/> like subsequence.</remarks>*/
public class ApproachPoint : ProcedurePoint
{
    public int Id { get; set; }

    /**<summary>
    <c>Vertical Angle (VERT ANGLE)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.70.</remarks>*/
    public float VerticalAngle { get; set; }

    /// <inheritdoc cref="ApproachQualifiers"/>
    public ApproachQualifiers Qualifiers { get; set; }
}
