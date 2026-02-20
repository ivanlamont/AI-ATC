using System.Diagnostics;
namespace AIATC.Data.Models.Navigation;

/// <inheritdoc />
[DebuggerDisplay($"{{{nameof(Identifier)},nq}}, {nameof(Port)} - {{{nameof(Port)}}}")]
public class TerminalBeacon : Nondirect
{
    public int Id { get; set; }

    public Ground.Port Port { get; set; }
}
