using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
namespace AIATC.Data.Models.Routing;

/**<summary>
<c>Special Activity Area</c> primary record.
</summary>
<remarks>See section 4.1.33.1.</remarks>*/
public class SpecialArea : Fix, IIdentity, INamed
{
    public int Id { get; set; }

    public Ground.Port Port { get; set; }

    public ActivityType Type { get; set; }

    /**<summary>
    <c>Special Activity Area Size</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.280.</remarks>*/
    public float Size { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Limit']/*"/>
    public Altitude Up { get; set; }

    public LimitUnit UpUnit { get; set; }

    /// <summary><c>Special Activity Area Volume</c> character.</summary>
    /// <remarks>See section 5.281.</remarks>
    public char Volume { get; set; }

    public OperatingTimes Times { get; set; }

    public Privacy Privacy { get; set; }

    /// <summary><c>Controlling Agency</c> field.</summary>
    /// <remarks>See section 5.140.</remarks>
    public string? ControllingAgency { get; set; }

    /// <inheritdoc cref="Arinc424.CommType"/>
    public CommType CommType { get; set; }

    /// <inheritdoc cref="Arinc424.Frequency"/>
    public Frequency Frequency { get; set; }

    /// <summary><c>Restrictive Airspace Name</c> field.</summary>
    /// <remarks>See section 5.126.</remarks>
    public string? Name { get; set; }
}
