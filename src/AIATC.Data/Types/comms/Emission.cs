namespace AIATC.Data.Models.Types;

/// <summary>
/// Signal Emission (SIG EM) character.
/// </summary>
public enum Emission : byte
{
    Unknown,
    Double,
    SingleReducedCarrier,
    TwoIndependent,
    SingleFullCarrier,
    SingleSuppressedCarrier,
    LowerUnknownCarrier,
    UpperUnknownCarrier
}
