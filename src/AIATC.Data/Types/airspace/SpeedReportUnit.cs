namespace AIATC.Data.Models.Types;

/**<summary>
<c>FIR/UIR ATC Reporting Units Speed (RUS)</c> character.
</summary>
<remarks>See section 5.122.</remarks>*/
// ARINC mapping attributes removed
public enum SpeedReportUnit : byte
{
    Unknown,
    /**<summary>
    Not specified.
    </summary>*/
    Unspecified,
    Knots,
    Mach,
    KilometersPerHour
}
