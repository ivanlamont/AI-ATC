namespace AIATC.Data.Models.Procedures;

using AIATC.Data.Models.Types;

/**<summary>
<c>Airport</c> and <c>Heliport Approach</c> primary record sequence.
</summary>
<remarks>Used by <see cref="Approach"/> like subsequence.</remarks>*/
public class ApproachSequence : ProcedureSequence<ApproachPoint>
{
    public int Id { get; set; }

    /// <inheritdoc cref="ApproachTypes"/>
    public ApproachTypes Types { get; set; }
}
