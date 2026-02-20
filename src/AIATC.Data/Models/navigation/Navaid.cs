using System.Diagnostics;
namespace AIATC.Data.Models.Navigation;

[DebuggerDisplay($"{{{nameof(Identifier)},nq}}")]
public abstract class Navaid : Fix, INamed
{
    /// <include file='Comments.xml' path="doc/member[@name='Frequency']/*"/>
    public float Frequency { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Datum']/*"/>
    public string? Datum { get; set; }

    /// <include file='Comments.xml' path="doc/member[@name='Name']/*"/>
    public string? Name { get; set; }
}
