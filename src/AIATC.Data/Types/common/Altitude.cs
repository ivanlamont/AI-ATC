namespace AIATC.Data.Types.Common;

using Microsoft.EntityFrameworkCore;

/**<summary>
Various altitudes according to the specification.
</summary>*/
// ARINC mapping attributes removed
[Owned]
public class Altitude
{
    public float Value { get; set; }
    public AltitudeUnit Unit { get; set; }

    public Altitude(float Value, AltitudeUnit Unit)
    {
        this.Value = Value;
        this.Unit = Unit;
    }

    public static Altitude operator *(Altitude left, int right) => new(left.Value * right, left.Unit);
}
