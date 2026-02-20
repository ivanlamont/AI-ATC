namespace AIATC.Data.Models.Types;

/// <summary>
/// ILS/MLS/GLS Category (CAT) character.
/// </summary>
public enum LandingType : byte
{
    Unknown,
    NoGlideSlope,
    CategoryOne,
    CategoryTwo,
    CategoryThree,
    InstrumentGuidance,
    DirectionalGlideSlope,
    DirectionalNoGlideSlope,
    SimplifiedGlideSlope,
    SimplifiedNoGlideSlope
}
