namespace AIATC.Data.Models.Types;

/// <summary>
/// Third character of NAVAID Class (CLASS) field, specific to Omnidirect.
/// </summary>
public enum OmnidirectCoverage : byte
{
    Unknown,
    Terminal,
    LowAltitude,
    HighAltitude,
    Undefined,
    Tactical
}
