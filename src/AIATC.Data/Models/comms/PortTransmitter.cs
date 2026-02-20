using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using System.Diagnostics;
namespace AIATC.Data.Models.Comms;

/**<summary>
Fields of <c>Airport Communications</c> and <c>Heliport Communications</c>.
</summary>
<remarks>Used by <see cref="PortCommunication"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{{{nameof(CallSign)},nq}}, {nameof(Type)} - {{{nameof(Type)}}}")]
public class PortTransmitter : Transmitter
{
    public int Id { get; set; }

    /**<summary>
    <c>Multi-Sector Indicator (MSEC IND)</c> character.
    </summary>
    <remarks>See section 5.286.</remarks>*/
    public Bool IsMultiSector { get; set; }

    /**<summary>
    <c>Sectorization (SECTOR)</c> field.
    </summary>
    <remarks>See section 5.183.</remarks>*/
    public Sectorization? Sectorization { get; set; }

    public Fix? Facility { get; set; }

    /// <inheritdoc cref="DistanceLimitation"/>
    public DistanceLimitation Limitation { get; set; }

    /**<summary>
    <c>Communications Distance (COMM DIST)</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.188.</remarks>*/
    public int Distance { get; set; }

    /// <inheritdoc cref="PortCommUsages"/>
    public PortCommUsages Usages { get; set; }
}
