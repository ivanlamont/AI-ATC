namespace AIATC.Data.Types.Common;

using T = Privacy;

/// <summary>
/// Public/Military Indicator (PUB/MIL) character.
/// </summary>
public enum Privacy : byte
{
    Unknown = 0,
    Civil = 1,
    Military = 1 << 1,
    Private = 1 << 2
}
