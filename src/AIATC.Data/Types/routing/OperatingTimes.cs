// namespace removed for migration

/**<summary>
<c>Special Activity Area Operating Times</c> field.
</summary>
<remarks>See section 5.282.</remarks>*/
[Flags]

public enum OperatingTimes : ushort
{
    Unknown = 0,
    ContinuousDays = 1,
    Weekdays = 1 << 1,
    Weekends = 1 << 2,
    OtherDays = 1 << 3,
    DaysUnspecified = 1 << 4,

    WithHolidays = 1 << 5,
    WithoutHolidays = 1 << 6,
    HolidaysUnspecified = 1 << 7,

    SunriseSunset = 1 << 8,
    Night = 1 << 9,
    ContinuousTimes = 1 << 10,
    Notam = 1 << 11
}
