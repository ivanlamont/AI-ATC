namespace AIATC.Data.Models.Types;

/// <summary>
/// Communications Class (Comm Class) field.
/// </summary>
public enum CommClass : byte
{
    Unknown,
    RegionControl,
    RegionInfo,
    OtherInfoControl,
    Broadcast,
    Terminal,
    Ground,
    OtherGroundTerminal,
    GroundTerminalBroadcast
}
