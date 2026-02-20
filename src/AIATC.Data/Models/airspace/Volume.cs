
using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
namespace AIATC.Data.Models.Airspace;

/**<summary>
Space volume with low and up limits.
</summary>*/
public abstract class Volume : Record424<BoundaryPoint>, IMultiple
{
    public char? Multiplier { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.LevelType"/>
    public LevelType LevelType { get; set; }

    /// <inheritdoc cref="AIATC.Data.Models.TimeCode"/>
    public TimeCode TimeCode { get; set; }

    /// <summary><c>NOTAM</c> character.</summary>
    /// <remarks>See section 5.132.</remarks>
    public char Notam { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude Low { get; set; }

    /// <inheritdoc cref="LimitUnit"/>
    public LimitUnit LowUnit { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude Up { get; set; }

    /// <inheritdoc cref="LimitUnit"/>
    public LimitUnit UpUnit { get; set; }
}
