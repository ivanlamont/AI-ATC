namespace AIATC.Data.Models.Procedures;

using AIATC.Data.Models.Types;

/**<summary>
<c>Airport</c> and <c>Heliport SID</c> primary record sequence.
</summary>
<remarks>Used by <see cref="Departure"/> like subsequence.</remarks>*/
public class DepartureSequence : ProcedureSequence<DeparturePoint>
{
    public int Id { get; set; }

    /// <inheritdoc cref="DepartureTypes"/>
    public DepartureTypes Types { get; set; }
}
