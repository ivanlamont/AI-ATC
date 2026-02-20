using System.Diagnostics;
namespace AIATC.Data.Models.Ground;

/**<summary>
<c>Airport Gate</c> primary record.
</summary>
<remarks>See section 4.1.8.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class Gate : Fix, INamed
{
    public int Id { get; set; }

    public Airport Port { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Name']/*"/>
    public string? Name { get; set; }
}
