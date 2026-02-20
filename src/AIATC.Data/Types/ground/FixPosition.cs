namespace AIATC.Data.Models.Types;

/**<summary>
<c>Fix Position Indicator</c> character.
</summary>
<remarks>See section 5.272.</remarks>*/
// ARINC mapping attributes removed
public enum FixPosition : byte
{
    Unknown,
    /**<summary>
     Straight-In or Center Fix.
     </summary>*/
    Center,
    /**<summary>
    Left Base Area.
    </summary>*/
    Left,
    /**<summary>
    Right Base Area.
    </summary>*/
    Right
}
