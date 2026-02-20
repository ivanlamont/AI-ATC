// namespace removed for migration

/// <summary>
/// Waypoint Usage field.
/// </summary>
public enum WaypointUsages : byte
{
    Unknown = 0,
    AreaNavigation = 1,
    LowHigh = Low | High,
    High = 1 << 1,
    Low = 1 << 2,
    Terminal = 1 << 3
}
