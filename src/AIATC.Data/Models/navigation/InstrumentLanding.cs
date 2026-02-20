namespace AIATC.Data.Models.Navigation;

using AIATC.Data.Types.Common;

/**<summary>
<c>Airport and Heliport Localizer and Glide Slope</c> primary record.
</summary>
<remarks>See section 4.1.11.1.</remarks>*/
public class InstrumentLanding : Landing
{
    public int Id { get; set; }

    /**<summary>
    <c>Localizer Frequency (FREQ)</c> field.
    </summary>
    <value>Kilohertz.</value>
    <remarks>See section 5.45.</remarks>*/
    public int Frequency { get; set; }

    public Coordinates GlideSlopeCoordinates { get; set; }

    /**<summary>
    <c>Localizer Position (LOC FR RW END)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.48.</remarks>*/
    public int Position { get; set; }

    [Obsolete("todo - combine with position")]
    public char PositionReference { get; set; }

    /**<summary>
    <c>Glide Slope Position (GS FR RW THRES)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.50.</remarks>*/
    public int GlideSlopePosition { get; set; }

    /**<summary>
    <c>Localizer Width (LOC WIDTH)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.51.</remarks>*/
    public float Width { get; set; }

    /**<summary>
    <c>Glide Slope Angle (GS ANGLE)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.52.</remarks>*/
    public float SlopeAngle { get; set; }

    [Obsolete("todo")]
    public string Declination { get; set; }

    public Fix? SupportingFacility { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='TCH']/*"/>
    public int ThresholdHeight { get; set; }

    /// <summary>Associated ILS Markers.</summary>
    public InstrumentMarker[]? Markers { get; set; }
}
