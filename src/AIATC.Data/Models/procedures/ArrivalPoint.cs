using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Procedures;

/**<summary>
Fields of <c>Airport</c> and <c>Heliport STAR</c>.
</summary>
<remarks>Used by <see cref="ArrivalSequence"/> like subsequence.</remarks>*/
public class ArrivalPoint : ProcedurePoint
{
    public int Id { get; set; }

    /// <inheritdoc cref="ApproachPoint.VerticalAngle"/>
    public float VerticalAngle { get; set; }

    /// <inheritdoc cref="ArrivalQualifiers"/>
    public ArrivalQualifiers Qualifiers { get; set; }
}
