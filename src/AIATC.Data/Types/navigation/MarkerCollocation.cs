namespace AIATC.Data.Models.Types;

/**<summary>
Fifth character of <c>NAVAID Class (CLASS)</c> field
specific to <see cref="InstrumentMarker"/>.
</summary>
<remarks>See section 5.35.</remarks>*/
// ...existing code...
public enum MarkerCollocation : byte
{
    Unknown,
    /// <inheritdoc cref="NondirectCollocation.BeatFrequencyOscillator"/>
    BeatFrequencyOscillator,
    /**<summary>
    The latitude/longitude position of the Locator and Marker are identical.
    </summary>*/
    Collocated,
    /**<summary>
    The latitude/longitude position of Locator and Marker are not identical.
    </summary>*/
    Non
}
