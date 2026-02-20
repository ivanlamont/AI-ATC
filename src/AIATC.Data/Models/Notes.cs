namespace AIATC.Data.Models;

/**<summary>
Basic continuation record with notes.
</summary>
<remarks>See section 5.91.</remarks>*/
public abstract class BaseContinuation : Record424
{
    public string? Notes { get; set; }
}

/**<summary>
<c>Enroute Airways</c> continuation record.
</summary>
<remarks>See section 4.1.6.2.</remarks>*/
public class AirwayContinuation : BaseContinuation
{
    public int Id { get; set; }
}
