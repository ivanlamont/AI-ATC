namespace AIATC.Data.Models.Navigation;

using AIATC.Data.Models.Types;

/**<summary>
<c>NDB Navaid</c> primary record.
</summary>
<remarks>See section 4.1.3.1.</remarks>*/
public class Nondirect : Navaid
{
    public int Id { get; set; }

    public NondirectType Type { get; set; }
    public NondirectCoverage Coverage { get; set; }
    public NondirectInfo Info { get; set; }
    public NondirectCollocation Collocation { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='MagneticVariation']/*"/>
    public float Variation { get; set; }
}
