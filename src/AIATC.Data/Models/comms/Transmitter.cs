using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
namespace AIATC.Data.Models.Comms;

public abstract class Transmitter : Geo, ISequenced
{
    public int SeqNumber { get; set; }

    /// <inheritdoc cref="CommType"/>
    public CommType Type { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.Frequency"/>
    public Frequency Frequency { get; set; }

    /// <summary><c>Radar (RADAR)</c> character.</summary>
    /// <remarks>See section 5.102.</remarks>
    public Bool IsRadarAvailable { get; set; }

    /// <summary><c>H24 Indicator (H24)</c> character.</summary>
    /// <remarks>See section 5.181.</remarks>
    public Bool IsWholeDay { get; set; }

    /// <summary><c>Call Sign (CALL SIGN)</c> field.</summary>
    /// <remarks>See section 5.105.</remarks>
    public string? CallSign { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.AltitudeDescription"/>
    public AltitudeDescription AltitudeDescription { get; set; }

    /**<summary>
    <c>Communication Altitude (COMM ALTITUDE)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.184.</remarks>*/
    public Altitude Altitude { get; set; }

    /// <inheritdoc cref="Altitude"/>
    public Altitude Altitude2 { get; set; }

    /// <inheritdoc cref="Modulation"/>
    public Modulation Modulation { get; set; }

    /// <inheritdoc cref="Emission"/>
    public Emission Emission { get; set; }
}
