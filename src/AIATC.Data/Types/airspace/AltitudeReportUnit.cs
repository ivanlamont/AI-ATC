namespace AIATC.Data.Models.Types;

/**<summary>
<c>FIR/UIR ATC Reporting Units Altitude (RUA)</c> character.
</summary>
<remarks>See section 5.123.</remarks>*/
// ...existing code...
public enum AltitudeReportUnit : byte
{
    Unknown,
    Unspecified,
    Level,
    Meters,
    Feet
}
