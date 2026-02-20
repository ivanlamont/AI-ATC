namespace AIATC.Data.Models.Tables;

using AIATC.Data.Models.Types;
using System.Diagnostics;

/**<summary>
<c>Cruising Table</c> record sequence.
</summary>
<remarks>See section 4.1.16.1.</remarks>*/
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public class CruiseTable : Record424<CruiseColumn>, IIdentity
{
    public int Id { get; set; }

    /// <summary><c>Cruise Table Identifier (CRSE TBL IDENT)</c> field.</summary>
    /// <remarks>See section 5.134.</remarks>
    public string Identifier { get; set; }
}
