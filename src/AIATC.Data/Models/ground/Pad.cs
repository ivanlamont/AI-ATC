using System.Diagnostics;
namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Helipad</c> primary record.
</summary>*/
[Obsolete("todo: describe supplement v21+")]
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class Pad : Touch
{
    public int Id { get; set; }

    public Port Port { get; set; }
}
