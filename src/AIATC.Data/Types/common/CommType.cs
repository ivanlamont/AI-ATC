namespace AIATC;

/// <summary>
/// Communications Type (COMM TYPE) field.
/// </summary>
public enum CommType : byte
{
    Unknown,
    Area,
    Airlift,
    Air,
    Approach,
    Arrival,
    SurfaceObserving,
    TerminalInfoService,
    WeatherBroadcast,
    WeatherObserving,
    WeatherServices,
    Bravo,
    Charlie,
    DeliveryClearance,
    PreTaxiClearance,
    ControlArea,
    CommonTrafficAdvisory,
    Control,
    Departure,
    Director,
    FlightAdvisory,
    Emergency,
    FlightService,
    /**<summary>
    Ground Comm Outlet.
    </summary>*/
    GroundOutlet,
    /**<summary>
    Ground Control.
    </summary>*/
    Ground,
    /**<summary>
    Gate Control.
    </summary>*/
    Gate,
    /**<summary>
    Helicopter Frequency.
    </summary>*/
    Helicopter,
    /**<summary>
    Information.
    </summary>*/
    Information,
    /**<summary>
    Mandatory Broadcast Zone.
    </summary>*/
    BroadcastZone,
    /**<summary>
    Military Frequency.
    </summary>*/
    Military,
    /**<summary>
    Multicom.
    </summary>*/
    Multicom,
    /**<summary>
    Operations.
    </summary>*/
    Operations,
    /**<summary>
    Pilot Activated Lighting.
    </summary>*/
    ActivatedLighting,
    /**<summary>
    Radio.
    </summary>*/
    Radio,
    /**<summary>
    Radar.
    </summary>*/
    Radar,
    /**<summary>
    Remote Flight Service Station (RFSS).
    </summary>*/
    RemoteFlightService,
    /**<summary>
    Ramp/Taxi Control.
    </summary>*/
    RampTaxi,
    /**<summary>
    Airport Radar Service Area (ARSA).
    </summary>*/
    RadarService,
    /**<summary>
    Terminal Control Area (TCA).
    </summary>*/
    TerminalControlArea,
    /**<summary>
    Terminal Control Area (TMA).
    </summary>*/
    TerminalManeuveringArea,
    Terminal,
    TerminalRadarService,
    TranscriberWeatherBroadcast,
    Tower,
    UpperArea,
    Unicom,
    Volmet
}
