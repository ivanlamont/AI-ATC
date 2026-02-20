namespace AIATC.Data.Models.Types;

/// <summary>
/// Service Indicator (SERV IND) - Enroute.
/// </summary>
[System.Flags]
public enum AirwayCommUsages : ushort
{
    Unknown = 0,
    AeronauticalInfo = 1,
    FlightInfo = 1 << 1,
    AirGround = 1 << 2,
    Discrete = 1 << 3,
    AirToAir = 1 << 4,
    Mandatory = 1 << 5,
    Secondary = 1 << 6,
    DirectionFinding = 1 << 7,
    RemoteAirToGround = 1 << 8,
    NonEnglish = 1 << 9,
    Military = 1 << 10,
    RemoteOutlet = 1 << 11
}
