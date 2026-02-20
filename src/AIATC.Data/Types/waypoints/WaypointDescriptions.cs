// namespace removed for migration

/// <summary>
/// Waypoint Description Code field.
/// </summary>
public enum WaypointDescriptions : ulong
{
    Unknown = 0u,
    Airport = 1u,
    Essential = 1u << 1,
    OffAirway = 1u << 2,
    RunwayHelipad = 1u << 3,
    Heliport = 1u << 4,
    Nondirectional = 1u << 5,
    Phantom = 1u << 6,
    Nonessential = 1u << 7,
    TransitionEssential = 1u << 8,
    Omnidirectional = 1u << 9,
    Ending = 1u << 10,
    ContinuousSegmentEnd = 1u << 11,
    UnchartedIntersection = 1u << 12,
    FlyOver = 1u << 13,
    StepdownFinal = 1u << 14,
    StepdownIntermediate = 1u << 15,
    ReportingPoint = 1u << 16,
    OceanicGateway = 1u << 17,
    MissedApproachFirstLeg = 1u << 18,
    TurnFinalApproach = 1u << 19,
    NamedStepdown = 1u << 20,
    InitialApproach = 1u << 21,
    IntermediateApproach = 1u << 22,
    HoldInitialApproach = 1u << 23,
    InitialApproachFacf = 1u << 24,
    FinalEndpoint = 1u << 25,
    FinalApproach = 1u << 26,
    WithoutHolding = 1u << 27,
    WithHolding = 1u << 28,
    FinalApproachCourse = 1u << 29,
    MissedApproach = 1u << 30,
    EngineOut = 1u << 31,
    InitialDeparture = 1ul << 32,
    QuietClimb = 1ul << 33
}
