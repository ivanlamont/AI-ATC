using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Airspace;

/**<summary>
Fields of <c>FIR/UIR</c>.
</summary>
<remarks>Used by <see cref="RegionVolume"/> like subsequence.</remarks>*/
public class RegionPoint : BoundaryPoint
{
    public int Id { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='FIR']/*"/>
    public string? Adjacent { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='FIR']/*"/>
    public string? UpperAdjacent { get; set; }
}
