namespace AIATC.Data.Models.Types;

/**<summary>
<c>Distance Description (DIST DESC)</c> character.
</summary>
<remarks>See section 5.187.</remarks>*/
// ...existing code...
public enum DistanceLimitation : byte
{
    Unknown,
    /**<summary>
    No restrictions/limitations apply.
    </summary>*/
    None,
    /**<summary>
    Communications frequency or navaid limitation is out to a specified distance.
    </summary>*/
    Out,
    /**<summary>
    Communications frequency is used or the navaid limitation applies beyond a specified distance.
    </summary>*/
    Beyond
}
