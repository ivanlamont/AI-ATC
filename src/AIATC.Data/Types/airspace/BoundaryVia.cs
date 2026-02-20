namespace AIATC.Data.Models.Types;

/**<summary>
<c>Boundary Via (BDRY VIA)</c> character.
</summary>
<remarks>See section 5.118.</remarks>*/
// ...existing code...
public enum BoundaryVia : byte
{
    Unknown,
    Circle,
    GreatCircle,
    RhumbLine,
    CounterClockwiseArc,
    ClockwiseArc
}
