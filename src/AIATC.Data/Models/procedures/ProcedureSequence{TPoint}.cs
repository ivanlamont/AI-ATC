using AIATC.Data.Models.Types;
using System.Diagnostics;
namespace AIATC.Data.Models.Procedures;

/**<summary>
<c>SID/STAR/Approach</c> primary record sequence.
</summary>
<remarks>See section 4.1.9.1 and 4.2.3.1.</remarks>*/
[DebuggerDisplay($"{nameof(Transition)} - {{{nameof(Transition)},nq}}")]
public abstract class ProcedureSequence<TPoint> : Record424<TPoint>
    where TPoint : ProcedurePoint
{
    /// <summary><c>Transition Identifier (TRANS IDENT)</c> field.</summary>
    /// <remarks>See section 5.11.</remarks>
    public string? Transition { get; set; }

    /// <inheritdoc cref="AircraftTypes"/>
    public AircraftTypes AircraftTypes { get; set; }
}
