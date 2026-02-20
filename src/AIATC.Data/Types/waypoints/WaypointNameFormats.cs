// namespace removed for migration

/// <summary>
/// Name Format Indicator field.
/// </summary>
public enum WaypointNameFormats : uint
{
    Unknown = 0,
    Abeam = 1u,
    BearingDistance = 1u << 1,
    AirportName = 1u << 2,
    FlightInfoRegion = 1u << 3,
    PhoneticLetterName = 1u << 4,
    AirportIdentifier = 1u << 5,
    LatitudeLongitude = 1u << 6,
    MultipleWordName = 1u << 7,
    NavaidIdentifier = 1u << 8,
    FiveLetterName = 1u << 9,
    LessFiveLetterName = 1u << 10,
    MoreFiveLetterName = 1u << 11,
    AirportRunway = 1u << 12,
    UpperInfo = 1u << 13,
    Checkpoint = 1u << 14,
    LocalizerOfficialFive = 1u << 15,
    LocalizerUnofficialFive = 1u << 16
}
