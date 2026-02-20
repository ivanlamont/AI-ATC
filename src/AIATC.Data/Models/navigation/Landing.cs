using AIATC.Data.Models.Types;
using AIATC.Data.Models.Ground;
using System.Diagnostics;

namespace AIATC.Data.Models.Navigation;

[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public abstract class Landing : Fix
{
    public Ground.Port Port { get; set; }

    public Ground.Touch Touch { get; set; }

    public LandingType Type { get; set; }

    /**<summary>
    <para>
      <c>Localizer Bearing (LOC BRG)</c> field for <see cref="InstrumentLanding"/> and <see cref="GlobalLanding"/>.
    </para>
    <para>
      <c>MLS Azimuth Bearing (MLS AZ BRG)</c> field for <see cref="MicrowaveLanding"/>.
    </para>
    </summary>
    <remarks>See section 5.47 and 5.167.</remarks>*/
    public Course Bearing { get; set; }

    /**<summary>
    <c>Component Elevation</c> field.
    </summary>
    <remarks>See section 5.74.</remarks>*/
    public int Elevation { get; set; }
}
