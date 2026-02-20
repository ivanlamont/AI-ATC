namespace AIATC.Data.Types.Navigation;

/// <summary>
/// VOR/DME/TACAN Type character.
/// </summary>
[System.Flags]
public enum OmnidirectType : byte
{
    Unknown = 0,
    VOR = 1,
    VORDME = 1 << 1,
    DME = 1 << 2,
    TACAN = 1 << 3,
    VORTAC = 1 << 4,
    ILS_DME = 1 << 5,
    ILS_TACAN = 1 << 6
}
