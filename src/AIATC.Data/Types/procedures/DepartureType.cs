namespace AIATC.Data.Models.Types;

using T = DepartureTypes;

/**<summary>
<c>Route Type (RT TYPE)</c> -> <c>SID Route Type Description</c> character.
</summary>
<remarks>See section 5.7, Table 5-5.</remarks>*/
[Flags]

public enum DepartureTypes : byte
{
    Unknown = 0,
    /**<summary>
    Engine Out SID.
    </summary>*/
    EngineOut = 1,
    /**<summary>
    SID Runway Transition.
    </summary>*/
    // ...existing code...
    // ...existing code...
    // ...existing code...
    // ...existing code...
    // ...existing code...
    Runway = 1 << 1,
    /**<summary>
    SID or SID Common Route.
    </summary>*/
    // ...existing code...
    // ...existing code...
    // ...existing code...
    // ...existing code...
    Common = 1 << 2,
    /**<summary>
    SID Enroute Transition.
    </summary>*/
    // ...existing code...
    // ...existing code...
    // ...existing code...
    // ...existing code...
    // ...existing code...
    Enroute = 1 << 3,
    /**<summary>
    RNAV SID.
    </summary>*/
    AreaNavigation = 1 << 4,
    /**<summary>
    FMS SID.
    </summary>*/
    FlightManagement = 1 << 5,
    /**<summary>
    RNP SID.
    </summary>*/
    Performance = 1 << 6,
    /**<summary>
    Vector SID.
    </summary>*/
    Vector = 1 << 7
}
