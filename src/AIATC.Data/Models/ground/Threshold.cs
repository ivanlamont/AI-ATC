using System.Diagnostics;
using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Runway</c> primary record.
</summary>
<remarks>See section 4.1.10.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class Threshold : Touch
{
    public int Id { get; set; }

    public Airport Port { get; set; }

    /**<summary>
    <c>Runway Length (RUNWAY LENGTH)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.57.</remarks>*/
    public int Length { get; set; }

    /**<summary>
    <c>Runway Bearing (RWY BRG)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.58.</remarks>*/
    public Course Bearing { get; set; }

    /**<summary>
    <c>Runway Gradient (RWY GRAD)</c> field.
    </summary>
    <value>Degrees.</value>
    <remarks>See section 5.212.</remarks>*/
    [Obsolete("todo")]
    public float Gradient { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='EllipsoidalHeight']/*"/>
    public float EllipsoidalHeight { get; set; }

    /**<summary>
    <c>Landing Threshold Elevation (LANDING THRES ELEV)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.68.</remarks>*/
    public int Elevation { get; set; }

    /**<summary>
    <c>Threshold Displacement Distance (DSPLCD THR)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.69.</remarks>*/
    public int Distance { get; set; }

    /**<summary>
    <c>Runway Width (WIDTH)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.109.</remarks>*/
    public int Width { get; set; }

    /// <inheritdoc cref="Terms.ThresholdType"/>
    public ThresholdType Type { get; set; }

    /**<summary>
    <c>Stopway</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.79.</remarks>*/
    public int Stopway { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='TCH']/*"/>
    public int Height { get; set; }

    /**<summary>
    <c>Runway Description (RUNWAY DESCRIPTION)</c> field.
    </summary>
    <remarks>See section 5.59.</remarks>*/
    public string? Description { get; set; }
}
