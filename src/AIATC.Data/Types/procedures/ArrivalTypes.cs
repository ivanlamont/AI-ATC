namespace AIATC.Data.Models.Types;

using T = ArrivalTypes;

/**<summary>
<c>Route Type (RT TYPE)</c> -> <c>STAR Route Type Description</c> character.
</summary>
<remarks>See section 5.7, Table 5-7.</remarks>*/
[Flags]
public enum ArrivalTypes : byte
{
    Unknown = 0,
    /**<summary>
    STAR Enroute Transition.
    </summary>*/
    // ...existing code...
    Enroute = 1,
    /**<summary>
    STAR Common Route.
    </summary>*/
    // ...existing code...
    Common = 1 << 1,
    /**<summary>
    STAR Runway Transition.
    </summary>*/
    // ...existing code...
    Runway = 1 << 2,
    /**<summary>
    Profile Descent STAR.
    </summary>*/
    Descent = 1 << 4,
    /**<summary>
    RNP STAR.
    </summary>*/
    Performance = 1 << 5,
    /**<summary>
    RNAV STAR.
    </summary>*/
    AreaNavigation = 1 << 6,
    /**<summary>
    FMS STAR.
    </summary>*/
    FlightManagement = 1 << 7
}
