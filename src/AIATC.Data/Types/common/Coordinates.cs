namespace AIATC.Data.Types.Common;

using Microsoft.EntityFrameworkCore;

/**<summary>
Various latitudes and longitudes according to the specification.
</summary>
<remarks>See section 5.36, 5.37, 5.267, 5.268.</remarks>*/

[Owned]
public class Coordinates
{
    /**<summary>
    <c>Latitude (LATITUDE)</c> or <c>High Precision Latitude (HPLAT)</c> field.
    </summary>
    <remarks>See section 5.36 or 5.267.</remarks>*/
    public double Latitude { get; set; }

    /**<summary>
    <c>Longitude (LONGITUDE)</c> or <c>High Precision Longitude (HPLONG)</c> field.
    </summary>
    <remarks>See section 5.37 or 5.268.</remarks>*/
    public double Longitude { get; set; }

    public Coordinates(double Latitude, double Longitude)
    {
        this.Latitude = Latitude;
        this.Longitude = Longitude;
    }
}
