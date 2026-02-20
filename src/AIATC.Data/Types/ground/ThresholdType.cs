namespace AIATC.Data.Models.Types;

/**<summary>
<c>TCH Value Indicator (TCHVI)</c> character.
</summary>
<remarks>See section 5.270.</remarks>>*/
// ARINC mapping attributes removed
public enum ThresholdType : byte
{
    Unknown,
    /**<summary>
    TCH provided in Runway Record is that of the Electronic Glide Slope.
    </summary>*/
    ElectronicGlideSlope,
    /**<summary>
    TCH provided in Runway Record is that of an RNAV procedure to the runway.
    </summary>*/
    AreaNavigation,
    /**<summary>
    TCH Provided in the Runway Record is that of the VGSI for the runway
    </summary>*/
    Visual,
    /**<summary>
    TCH provided in the Runway Record is the default value of 40 or 50 feet.
    </summary>
    <remarks>See section 5.67.</remarks>*/
    Default
}
