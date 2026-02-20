namespace AIATC.Data.Types.Navigation;

/// <summary>
/// Navaid Usable Range character.
/// </summary>
/// <remarks>See section 5.149.</remarks>
[System.Flags]
public enum UsableRange : byte
{
    Unknown = 0,
    Terminal = 1,           // T - 25 NM
    LowAltitude = 1 << 1,   // L - 40 NM
    HighAltitude = 1 << 2,  // H - 130 NM
    Extended = 1 << 3,      // E - 200 NM
    Unlimited = 1 << 4      // U - Unlimited
}
