using System.Diagnostics;
using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;

namespace AIATC.Data.Models.Airspace;

/**<summary>
<c>FIR/UIR</c> primary record sequence.
</summary>
<remarks>Used by <see cref="FlightRegion"/> like subsequence.</remarks>*/
[DebuggerDisplay($"{{{nameof(Type)},nq}}")]
public class RegionVolume : Record424<RegionPoint>
{
    public int Id { get; set; }

    public Tables.CruiseTable? CruiseTable { get; set; }

    /// <inheritdoc cref="RegionType"/>
    public RegionType Type { get; set; }

    /// <inheritdoc cref="SpeedReportUnit"/>
    public SpeedReportUnit SpeedReportUnit { get; set; }

    /// <inheritdoc cref="AltitudeReportUnit"/>
    public AltitudeReportUnit AltitudeReportUnit { get; set; }

    /**<summary>
    <c>FIR/UIR Entry Report (ENTRY)</c> character.
    </summary>
    <remarks>See section 5.124.</remarks>*/
    public Bool IsEntryReport { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude Up { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude UpperRegionLow { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude UpperRegionUp { get; set; }
}
