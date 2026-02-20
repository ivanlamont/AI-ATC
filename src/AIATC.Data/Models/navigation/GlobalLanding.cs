namespace AIATC.Data.Models.Navigation;

/**<summary>
<c>GLS</c> primary record.
</summary>
<remarks>See section 4.1.29.1.</remarks>*/
public class GlobalLanding : Landing
{
    public int Id { get; set; }

    /// <summary><c>GLS Channel</c> field.</summary>
    /// <remarks>See section 5.244.</remarks>
    public int Channel { get; set; }

    /**<summary>
    <c>Service Volume Radius</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.245.</remarks>*/
    public int Radius { get; set; }

    /// <summary><c>TDMA Slots </c> field.</summary>
    /// <remarks>See section 5.246.</remarks>
    public byte Slots { get; set; }

    /**<summary>
    <c>Glide Slope Angle (GS ANGLE)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.52.</remarks>*/
    public float SlopeAngle { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Datum']/*"/>
    public string? Datum { get; set; }

    /// <summary><c>Station Type</c> field.</summary>
    /// <remarks>See section 5.247.</remarks>
    [Obsolete("todo")]
    public string? StationType { get; set; }

    /**<summary>
    <c>Station Elevation WGS84</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.248.</remarks>*/
    public int ElevationWgs84 { get; set; }
}
