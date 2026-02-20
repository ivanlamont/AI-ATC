namespace AIATC.Data.Models.Routing;

using AIATC.Data.Models.Types;
using System.Diagnostics;

/**<summary>
<c>Enroute Airways</c> primary record sequence.
</summary>
<remarks>See section 4.1.6.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public class Airway : Record424<AirwayPoint>, IIdentity
{
    public int Id { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Route']"/>
    public string Identifier { get; set; }
}
