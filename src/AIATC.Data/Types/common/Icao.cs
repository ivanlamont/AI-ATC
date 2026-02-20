namespace AIATC.Data.Types.Common;

using Microsoft.EntityFrameworkCore;

/**<summary>
Two letter ICAO code.
</summary>
<remarks>See section 5.14.</remarks>*/
// ARINC mapping attributes removed
[Owned]
public class Icao
{
    public char First { get; set; }
    public char Second { get; set; }

    public Icao(char First, char Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public override string ToString() => new([First, Second]);

    public void Deconstruct(out char first, out char second)
    {
        first = First;
        second = Second;
    }
}
