namespace AIATC.Data.Models.Ground;

/**<summary>
<c>GBAS Path Point</c> primary record.
</summary>
<remarks>See section 4.1.35.1.</remarks>*/
public class GroundPoint : PathPoint
{
    public int Id { get; set; }

    /// <inheritdoc cref="Terms.GroundOperationType"/>
    public string Type { get; set; }
}
