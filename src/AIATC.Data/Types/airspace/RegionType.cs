namespace AIATC.Data.Models.Types;

using T = RegionType;

/**<summary>
<c>FIR/UIR Indicator (IND)</c> character.
</summary>
<remarks>See section 5.117.</remarks>*/
// ARINC mapping attributes removed
public enum RegionType : byte
{
    Unknown = 0,
    /**<summary>
    FIR.
    </summary>*/
    Flight = 1,
    Upper = 1 << 1
}
