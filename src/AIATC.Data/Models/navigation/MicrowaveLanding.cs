namespace AIATC.Data.Models.Navigation;

using AIATC.Data.Types.Common;

/**<summary>
<c>Airport and Heliport MLS (Azimuth, Elevation and Back Azimuth)</c> primary record.
</summary>
<remarks>See section 4.1.22.1.</remarks>*/
public class MicrowaveLanding : Landing
{
    public int Id { get; set; }

    /// <summary><c>Channel</c> field.</summary>
    /// <remarks>See section 5.166.</remarks>
    public int Channel { get; set; }

    public Coordinates ElevationCoordinates { get; set; }

    /**<summary>
    <c>Azimuth/Back Azimuth Position (AZ/BAZ FR RW END)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.48.</remarks>*/
    public int Position { get; set; }

    /// <summary>
    /// <c>Azimuth Position Reference (@, +, -)</c> character.
    /// </summary>
    /// <remarks>See section 5.49.</remarks>
    [Obsolete("TODO combine with position")]
    public char PositionReference { get; set; }

    /**<summary>
    <c>Elevation Position (EL FR RW THRES)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.50.</remarks>*/
    public int ElevationPosition { get; set; }

    /**<summary>
    <c>Azimuth Proportional Angle Right (AZ PRO RIGHT)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.168.</remarks>*/
    public int RightAngle { get; set; }

    /**<summary>
    <c>Azimuth Proportional Angle Left (AZ PRO LEFT)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.168.</remarks>*/
    public int LeftAngle { get; set; }

    /**<summary>
    <c>Azimuth Coverage Sector Right (AZ COV RIGHT)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.172.</remarks>*/
    public int RightCoverage { get; set; }

    /**<summary>
    <c>Azimuth Coverage Sector Left (AZ COV LEFT)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.172.</remarks>*/
    public int LeftCoverage { get; set; }

    /**<summary>
    <c>Elevation Angle Span (EL ANGLE SPAN)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.169.</remarks>*/
    public float AngleSpan { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }

    /**<summary>
    <c>Nominal Elevation Angle (NOM ELEV ANGLE)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.173.</remarks>*/
    public float NominalElevationAngle { get; set; }

    /**<summary>
    <c>Glide Slope Angle (GS ANGLE)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.52.</remarks>*/
    public float MinimumGlideAngle { get; set; }

    public Fix? SupportingFacility { get; set; }
}
