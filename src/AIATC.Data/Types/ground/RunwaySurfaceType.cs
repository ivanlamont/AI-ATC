namespace AIATC.Data.Models.Types;

/**<summary>
<c>Surface Code (SC)</c> character.
</summary>
<remarks>See section 5.249.</remarks>*/
// ARINC mapping attributes removed
public enum SurfaceType : byte
{
    Unknown,
    /**<summary>
    Surface material not provided in source.
    </summary>*/
    Unspecified,
    /**<summary>
    Hard runway, for example, asphalt or concrete.
    </summary>*/
    Hard,
    /**<summary>
    Soft runway, for example, gravel, grass or soil.
    </summary>*/
    Soft,
    /**<summary>
    Water runway.
    </summary>*/
    Water
}
