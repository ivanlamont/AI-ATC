namespace AIATC.Data.Models.Types;

/**<summary>
<c>Service Indicator (SERV IND)</c> -> <c>Airport/Heliport</c> field.
</summary>
<remarks>See section 5.106, Table 5-20.</remarks>*/
[Flags]

public enum PortCommUsages : uint
{
    Unknown = 0,
    /**<summary>
    Airport Advisory Service (AAS).
    </summary>*/
    Advisory = 1u,
    /**<summary>
    Community Aerodrome Radio Station (CARS).
    </summary>*/
    Community = 1u << 1,
    /**<summary>
    Departure Service (other than Departure Control Unit).
    </summary>*/
    Departure = 1u << 2,
    /**<summary>
    Flight Information Service (FIS).
    </summary>*/
    FlightInfo = 1u << 3,
    /**<summary>
    Initial Contact (IC).
    </summary>*/
    Initial = 1u << 4,
    /**<summary>
    Arrival Service (other than Arrival Control Unit).
    </summary>*/
    Arrival = 1u << 5,
    /**<summary>
    Pre-Departure Clearance (Data Link  Service).
    </summary>*/
    PreDepartureClearance = 1u << 6,
    /**<summary>
    Aerodrome Flight Information Service (AFIS).
    </summary>*/
    AerodromeFlightInfo = 1u << 7,
    /**<summary>
    Terminal Area Control (other than dedicated Terminal Control Unit).
    </summary>*/
    TerminalAreaControl = 1u << 8,
    /**<summary>
    Aerodrome Traffic Frequency (ATF).
    </summary>*/
    AerodromeTraffic = 1u << 9,
    /**<summary>
    Common Traffic Advisory Frequency (CTAF).
    </summary>*/
    CommonTraffic = 1u << 10,
    /**<summary>
    Mandatory Frequency (MF).
    </summary>*/
    Mandatory = 1u << 11,
    /**<summary>
    Air/Air.
    </summary>*/
    AirToAir = 1u << 12,
    /**<summary>
    Secondary Frequency.
    </summary>*/
    Secondary = 1u << 13,
    /**<summary>
    Air/Ground.
    </summary>*/
    AirGround = 1u << 14,
    /**<summary>
    VHF Direction Finding Service (VDF).
    </summary>*/
    DirectionFinding = 1u << 15,
    /**<summary>
    Remote Communications Air to Ground (RCAG).
    </summary>*/
    RemoteAirToGround = 1u << 16,
    /**<summary>
    Language other than English.
    </summary>*/
    NotEnglish = 1u << 17,
    /**<summary>
    Military Use Frequency.
    </summary>*/
    Military = 1u << 18,
    /**<summary>
    Pilot Controlled Light (PCL).
    </summary>*/
    ControlledLight = 1u << 19,
    /**<summary>
    Remote Communications Outlet (RCO).
    </summary>*/
    RemoteOutlet = 1u << 20
}
