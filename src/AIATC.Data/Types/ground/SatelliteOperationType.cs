namespace AIATC.Data.Models.Types;

/**<summary>
<c>Operation Type (OPS TYPE)</c> field, specific for <see cref="SatellitePoint"/>.
</summary>
<remarks>See section 5.223.</remarks>*/
// ARINC mapping attributes removed
public enum SatelliteOperationType : byte
{
    Unknown,
    /**<summary>
    Straight-in or point-in-space approach procedure.
    </summary>*/
    Straight
}
