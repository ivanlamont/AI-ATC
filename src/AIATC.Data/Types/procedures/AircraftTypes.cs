namespace AIATC.Data.Models.Types;

using T = AircraftTypes;

/**<summary>
<c> Procedure Design Aircraft Category or Type</c> character.
</summary>
<remarks>See section 5.301.</remarks>*/
// ...existing code...
public enum AircraftTypes : ushort
{
    Unknown = 0,

    Unspecified = 1,

    Alpha = 1 << 1,

    Bravo = 1 << 2,

    Charlie = 1 << 3,

    Delta = 1 << 4,

    Echo = 1 << 5,
    Piston = 1 << 6,
    Jet = 1 << 7,
    Turbojet = 1 << 8,
    Prop = 1 << 9,

    Turboprop = 1 << 10,

    Helicopter = 1 << 11,

    Unlimited = Alpha | Bravo | Charlie | Delta | Echo | Piston | Jet | Turbojet | Prop | Turboprop | Helicopter,

    NonJet = Piston | Turbojet | Prop | Turboprop,

    NonTurboJet = Piston | Jet | Prop | Turboprop
}
