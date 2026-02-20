namespace AIATC.Data.Models.Types;

using T = ApproachTypes;

/**<summary>
<c>Route Type (RT TYPE)</c> -> <c>Approach Route Type Description</c> character.
</summary>
<remarks>See section 5.7, Table 5-8.</remarks>*/
// Removed ARINC mapping attributes
[Flags]
public enum ApproachTypes : uint
{
    Unknown = 0,
    /**<summary>
    Approach Transition.
    </summary>*/
    Transition = 1,
    /**<summary>
    Localizer (LOC) Approach.
    </summary>*/
    // L
    [Sum<T>(Backcourse, 'B')]
    [Sum<T>(Directional, 'X')]
    Localizer = 1 << 1,
    /**<summary>
    Non-Directional Beacon (NDB) Approach.
    </summary>*/
    // N
    [Sum<T>(Equipment, 'Q')]
    Nondirect = 1 << 2,
    /**<summary>
    VOR Approach.
    </summary>*/
    // V
    [Sum<T>(Equipment, 'D')]
    [Sum<T>(Equipment | Tactical, 'S')]
    Omnidirect = 1 << 3,
    /**<summary>
    TACAN Approach.
    </summary>*/
    Tactical = 1 << 4,
    /**<summary>
    Flight Management System (FMS) Approach.
    </summary>*/
    FlightManagement = 1 << 5,
    /**<summary>
    Instrument Guidance System (IGS) Approach.
    </summary>*/
    InstrumentGuidance = 1 << 6,
    /**<summary>
    Instrument Landing System (ILS) Approach.
    </summary>*/
    InstrumentLanding = 1 << 7,
    /**<summary>
    GNSS Landing System (GLS) Approach.
    </summary>*/
    GlobalLanding = 1 << 8,
    /**<summary>
    Microwave Landing System (MLS) Approach.
    </summary>*/
    MicrowaveLanding = 1 << 9,
    /**<summary>
    Global Positioning System (GPS) Approach.
    </summary>*/
    GlobalPosition = 1 << 10,
    /**<summary>
    Area Navigation (RNAV) Approach.
    </summary>*/
    // R
    [Sum<T>(Performance, 'H')]
    AreaNavigation = 1 << 11,
    /**<summary>
    Simplified Directional Facility (SDF) Approach.
    </summary>*/
    Directional = 1 << 12,
    /**<summary>
    Approach Transition with TF Based Construction of RF Turns.
    </summary>*/
    BasedConstruction = 1 << 13,
    /**<summary>
    Missed Approach.
    </summary>*/
    Missed = 1 << 14,
    /**<summary>
    Localizer/Back Course Approach.
    </summary>*/
    Backcourse = 1 << 15,
    /**<summary>
    DME Approach.
    </summary>*/
    Equipment = 1 << 16,
    /**<summary>
    Required Navigation Performance (RNP) Approach.
    </summary>*/
    Performance = 1 << 17,
    /**<summary>
    Microwave Landing System (MLS), Type A Approach.
    </summary>*/
    TypeA = 1 << 18,
    /**<summary>
    Microwave Landing System (MLS), Type B Approach.
    </summary>*/
    TypeB = 1 << 19,
    /**<summary>
    Microwave Landing System (MLS), Type C Approach.
    </summary>*/
    TypeC = 1 << 20
}
