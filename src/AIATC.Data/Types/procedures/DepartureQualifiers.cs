namespace AIATC.Data.Models.Types;

/**<summary>
<c>Route Type (RT TYPE)</c> -> <c>SID Qualifiers</c> field.
</summary>
<remarks>See section 5.7, Table 5-5.</remarks>*/
[Flags]

public enum DepartureQualifiers : ushort
{
    Unknown = 0,
    /**<summary>
    DME required.
    </summary>*/
    DistanceEquipment = 1,
    /**<summary>
    GNSS required.
    </summary>*/
    GlobalNavigation = 1 << 1,
    /**<summary>
    Radar required.
    </summary>*/
    Radar = 1 << 2,
    /**<summary>
    Helicopter SID from runway.
    </summary>*/
    Helicopter = 1 << 3,
    /**<summary>
    RNP SAAAR/AR.
    </summary>*/
    NavPerformance = 1 << 4,
    /**<summary>
    VOR/DME RNAV.
    </summary>*/
    AreaNavigation = 1 << 5,
    /**<summary>
    Database supported RNAV.
    </summary>*/
    DatabaseAreaNavigation = 1 << 6,
    /**<summary>
    FMS required.
    </summary>*/
    FlightManagement = 1 << 7,
    /**<summary>
    Conventional Departures.
    </summary>*/
    Conventional = 1 << 8
}
