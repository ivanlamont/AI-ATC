using System.Diagnostics;
namespace AIATC.Data.Models.Waypoints;

/**<summary>
<c>Airport Waypoint</c> and <c>Heliport Waypoint</c> primary record.
</summary>
<remarks>See section 4.1.4.1 and 4.2.2.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class TerminalWaypoint : Waypoint
{
    public int Id { get; set; }

    public Ground.Port Port { get; set; }
}
