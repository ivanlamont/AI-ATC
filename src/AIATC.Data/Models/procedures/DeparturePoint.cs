using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Procedures;

/**<summary>
Fields of <c>Airport</c> and <c>Heliport SID</c>.
</summary>
<remarks>Used by <see cref="DepartureSequence"/> like subsequence.</remarks>*/
public class DeparturePoint : ProcedurePoint
{
    public int Id { get; set; }

    /// <inheritdoc cref="DepartureQualifiers"/>
    public DepartureQualifiers Qualifiers { get; set; }
}
