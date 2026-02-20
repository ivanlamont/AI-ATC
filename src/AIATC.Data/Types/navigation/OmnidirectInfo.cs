namespace AIATC.Data.Models.Types;

/// <summary>
/// Fourth character of NAVAID Class (CLASS) field, specific to Omnidirect.
/// </summary>
public enum OmnidirectInfo : byte
{
    Unknown,
    Biased,
    AutomaticBroadcast,
    ScheduledBroadcast,
    NoVoice,
    Voice
}
