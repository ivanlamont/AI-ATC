namespace AIATC.Data.Models.Ground;

using Procedures;
using System.Diagnostics;
using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;

/**<summary>
Fields of <c>GBAS Path Point</c> and <c>SBAS Path Point</c>.
</summary>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public abstract class PathPoint : Fix
{
    public Port Port { get; set; }

    public Approach Approach { get; set; }

    [Obsolete("todo")]
    public string AsRunway { get; set; }

    /**<summary>
    <c>Route Indicator (RTE IND)</c> character.
    </summary>
    <remarks>See section 5.224.</remarks>*/
    public char RouteIndicator { get; set; }

    /**<summary>
    <c>Reference Path Data Selector (REF PDS)</c> field.
    </summary>
    <remarks>See section 5.256.</remarks>*/
    public int PathSelector { get; set; }

    public ApproachPerformance ApproachPerformance { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='EllipsoidalHeight']/*"/>
    public float EllipsoidalHeight { get; set; }

    /**<summary>
    <c>Glide Path Angle (GPA)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.226.</remarks>*/
    public float GlideAngle { get; set; }

    /// <summary> Flight Path Alignment coordinates.</summary>
    public Coordinates AlignmentCoordinates { get; set; }

    /**<summary>
    <c>Course Width At Threshold (CRS WDTH)</c> field.
    </summary>
    <vallue>Meters.</vallue>
    <remarks>See section 5.228.</remarks>*/
    public float CourseWidth { get; set; }

    /**<summary>
    <c>Length Offset (OFFSET)</c> field.
    </summary>
    <value>Meters.</value>
    <remarks>See section 5.259.</remarks>*/
    public int LengthOffset { get; set; }

    /**<summary>
    <c>Path Point TCH</c> and <c>TCH Units Indicator</c> fields.
    </summary>
    <remarks>See section 5.265 and 5.266.</remarks>*/
    public Altitude ThresholdHeight { get; set; }

    /**<summary>
    <c>Final Approach Segment Data CRC Remainder (FAS CRC)</c> field.
    </summary>
    <remarks>See section 5.229.</remarks>*/
    [Obsolete("need to convert?")]
    public string Remainder { get; set; }
}
