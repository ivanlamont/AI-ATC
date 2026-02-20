namespace AIATC.Data.Models.Ground;

using Navigation;

public abstract class Touch : Fix
{
    /// <summary>Associated GLSs.</summary>
    public GlobalLanding[]? GlobalLandings { get; set; }

    /// <summary>Associated MLSs.</summary>
    public MicrowaveLanding[]? MicrowaveLandings { get; set; }

    /// <summary>Associated ILSs.</summary>
    public InstrumentLanding[]? InstrumentLandings { get; set; }

    /// <summary>Associated ILS Markers.</summary>
    public InstrumentMarker[]? Markers { get; set; }
}
