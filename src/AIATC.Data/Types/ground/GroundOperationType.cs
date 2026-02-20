namespace AIATC.Data.Models.Types;

/**<summary>
<c>Operation Type (OPS TYPE)</c> field, specific for <see cref="GroundPoint"/>.
</summary>
<remarks>See section 5.223.</remarks>*/
// ...existing code...
public enum GroundOperationType : byte
{
    Unknown,
    /**<summary>
    Straight-in approach path.
    </summary>*/
    Straight,
    /**<summary>
    Terminal Area path definition (not for FAS Datablock).
    </summary>*/
    Terminal,
    /**<summary>
    Missed Approach (not for FAS Datablock).
    </summary>*/
    Missed
}
