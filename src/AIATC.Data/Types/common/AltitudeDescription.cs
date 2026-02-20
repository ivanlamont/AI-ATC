namespace AIATC.Data.Types.Common;

/// <summary>
/// Altitude Description (ALT DESC) character.
/// </summary>
public enum AltitudeDescription : byte
{
    Unknown,
    AtAboveFirst,
    AtBelowFirst,
    AtFirst,
    AtAboveAtBelow,
    AtAboveSecond,
    NotBeforeAtAboveSecond,
    GlideSecondAtFirst,
    GlideSecondAtAboveFirst,
    GlideInterceptSecondAtFirst,
    GlideInterceptSecondAtAboveFirst,
    OptionalAtAbove,
    AtVerticalSecondAtAboveFirst,
    AtVerticalSecondAtFirst,
    AtVerticalSecondAtBelowFirst
}
