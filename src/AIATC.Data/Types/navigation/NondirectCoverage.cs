namespace AIATC.Data.Models.Types;

/**<summary>
Third character of <c>NAVAID Class (CLASS)</c> field,
specific to <see cref="Nondirect"/>.
</summary>
<remarks>See section 5.35.</remarks>*/
// ...existing code...
public enum NondirectCoverage : byte
{
    Unknown,
    /**<summary>
    Generally usable within 75NM of the facility at all altitudes.
    </summary>*/
    HighPowered,
    /**<summary>
    Generally usable within 50NM of the facility at all altitudes.
    </summary>*/
    Default,
    /**<summary>
    Generally usable within 25NM of the facility at all altitude.
    </summary>*/
    LowPowered,
    /**<summary>
    Generally usable within 15NM of the facility at all altitudes.
    </summary>*/
    Locator
}
