using System.ComponentModel;

namespace AIATC.Data.Models.Types;

/**<summary>
Fifth character of <c>NAVAID Class (CLASS)</c> field,
specific to <see cref="Nondirect"/>.
</summary>
<remarks>See section 5.35.</remarks>*/
// ARINC mapping attributes removed
[Description("NAVAID Class (CLASS) - Collocation")]
public enum NondirectCollocation : byte
{
    Unknown,
    /**<summary>
    Required to received an aural identification signal.
    </summary>*/
    BeatFrequencyOscillator
}
