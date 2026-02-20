namespace AIATC.Data.Models.Types;

/// <summary>
/// Fourth character of NAVAID Class (CLASS) field, specific to Nondirect.
/// </summary>
public enum NondirectInfo : byte
{
    Unknown,
    AutomaticBroadcast,
    ScheduledBroadcast,
    NoVoice,
    Voice
}
