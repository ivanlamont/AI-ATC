using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Procedures;

using AIATC.Data.Models.Types;

/**<summary>
<c>Airport</c> and <c>Heliport STAR</c> primary record sequence.
</summary>
<remarks>Used by <see cref="Arrival"/> like subsequence.</remarks>*/
public class ArrivalSequence : ProcedureSequence<ArrivalPoint>
{
    public int Id { get; set; }

    /// <inheritdoc cref="ArrivalTypes"/>
    public ArrivalTypes Types { get; set; }
}
