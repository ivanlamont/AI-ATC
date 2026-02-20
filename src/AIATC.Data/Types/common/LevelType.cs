namespace AIATC.Data.Types.Common;

using T = LevelType;

/// <summary>
/// Level (LEVEL) character.
/// </summary>
public enum LevelType : byte
{
    Unknown = 0,
    Low = 1,
    High = 1 << 1
}
