namespace AIATC.Data.Models.Types;

/**<summary>
First character of <c>Station Declination (STN DEC)</c>.
</summary>
<remarks>See section 5.66.</remarks>*/
// ...existing code...
public enum DeclinationType : byte
{
    Unknown,
    /**<summary>
    Declination is East of True North.
    </summary>*/
    East,
    /**<summary>
    Declination is West of True North.
    </summary>*/
    West,
    /**<summary>
    Station is oriented to True North in an area in which the local variation is not zero.
    </summary>*/
    True,
    /**<summary>
    Station is oriented to Grid North.
    </summary>*/
    Grid
}
