namespace AIATC.Data.Models.Comms;

using AIATC.Data.Models.Types;
using AIATC.Data.Models.Ground;
using System.Diagnostics;

/**<summary>
<c>Airport and Heliport Communications</c> primary record sequence.
</summary>
<remarks>See section 4.1.14.1 and 4.2.5.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Class)}}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class PortCommunication : Communication<PortTransmitter>
{
    public int Id { get; set; }

    public Ground.Port Port { get; set; }
}
