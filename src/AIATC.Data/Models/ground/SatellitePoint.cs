namespace AIATC.Data.Models.Ground;

/**<summary>
<c>SBAS Path Point</c> primary record.
</summary>
<remarks>See section 4.1.28.1 and 4.2.8.1.</remarks>*/
public class SatellitePoint : PathPoint
{
    public int Id { get; set; }

    /// <inheritdoc cref="Terms.SatelliteOperationType"/>
    public string Type { get; set; }

    /// <inheritdoc cref="Terms.SatelliteService"/>
    public string Service { get; set; }

    /**<summary>
    <c>HAL</c> field.
    </summary>
    <value>Meters.</value>
    <remarks>See section 5.263.</remarks>*/
    public float HorizontalAlert { get; set; }

    /**<summary>
    <c>VAL</c> field.
    </summary>
    <value>Meters.</value>
    <remarks>See section 5.264.</remarks>*/
    public float VerticalAlert { get; set; }
}
