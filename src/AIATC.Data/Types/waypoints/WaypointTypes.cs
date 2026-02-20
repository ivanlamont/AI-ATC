// namespace removed for migration

/// <summary>
/// Waypoint Type field.
/// </summary>
public enum WaypointTypes : ulong
{
    Unknown = 0,
    ArcCenter = 1,
    Combined = 1ul << 1,
    Charted = 1ul << 2,
    MiddleInner = 1ul << 3,
    Nondirect = 1ul << 4,
    OuterBack = 1ul << 5,
    IntersectionEquipment = 1ul << 6,
    AirwayIntersection = 1ul << 7,
    Visual = 1ul << 8,
    AreaNavigation = 1ul << 9,
    Final = 1ul << 10,
    InitialFinal = 1ul << 11,
    FinalCourse = 1ul << 12,
    Intermediate = 1ul << 13,
    OffRoute = 1ul << 14,
    OffRouteFaa = OffRoute,
    Initial = 1ul << 15,
    FinalCourseInitial = 1ul << 16,
    FinalCourseIntermediate = 1ul << 17,
    Missed = 1ul << 18,
    InitialMissed = 1ul << 19,
    OceanicGateway = 1ul << 20,
    Stepdown = 1ul << 21,
    NotAtProcedure = 1ul << 22,
    NamedStepdown = 1ul << 23,
    VolumeIntersection = 1ul << 24,
    FullLatitude = 1ul << 25,
    HalfLatitude = 1ul << 26,
    DepartureUse = 1ul << 27,
    ArrivalUse = 1ul << 28,
    ApproachUse = 1ul << 29,
    MultipleProcedureUse = 1ul << 30,
    Enroute = 1ul << 31
}
