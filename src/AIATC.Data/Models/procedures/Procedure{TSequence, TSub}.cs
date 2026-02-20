using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using System.Diagnostics;
namespace AIATC.Data.Models.Procedures;

/**<summary>
Fields of <c>Airport</c> and <c>Heliport SID/STAR/Approach</c>.
</summary>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {{{nameof(Port)}}}")]
public abstract class Procedure<TSequence, TSub> : Record424<TSequence>, IIdentity, IIcao
    where TSequence : ProcedureSequence<TSub>
    where TSub : ProcedurePoint
{
    public Ground.Port Port { get; set; }

    public Icao Icao { get; set; }

    /**<summary>
    <para>
      <c>Approach Route Identifier (APPROACH IDENT)</c> field for <see cref="Approach"/>.
    </para>
    <para>
      <c>SID/STAR Route Identifier (SID/STAR IDENT)</c> field for <see cref="Departure"/> and <see cref="Arrival"/>.
    </para>
    </summary>
    <remarks>See section 5.9 or 5.10.</remarks>*/
    public string Identifier { get; set; }
}
