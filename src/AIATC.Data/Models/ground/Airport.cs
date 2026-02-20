using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Airport</c> primary record.
</summary>
<remarks>See section 4.1.7.1.</remarks>*/
public class Airport : Port
{
    public int Id { get; set; }

    /**<summary>
    <c>Longest Runway (LONGEST RWY)</c> field.
    </summary>
    <value>Feet.</value>
    <remarks>See section 5.54.</remarks>*/
    public int LongestRunwayLength { get; set; }

    /// <inheritdoc cref="SurfaceType"/>
    public SurfaceType LongestRunwayType { get; set; }

    /// <summary>Associated gates.</summary>
    public Gate[]? Gates { get; set; }

    /// <summary>Associated runways.</summary>
    public Threshold[]? Thresholds { get; set; }
}
