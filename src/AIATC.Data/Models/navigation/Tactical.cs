using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Navigation;

/**<summary>
<c>TACAN-Only NAVAID</c> primary record.
</summary>
<remarks>See section 4.1.32.1.</remarks>*/
public class Tactical : Navaid
{
    public int Id { get; set; }

    public Ground.Port Port { get; set; }

    public OmnidirectType Type { get; set; }
    public OmnidirectCoverage Coverage { get; set; }
    public OmnidirectInfo Info { get; set; }
    public OmnidirectCollocation Collocation { get; set; }

    /// <inheritdoc cref="Omnidirect.EquipmentIdentifier"/>
    public string TacanIdentifier { get; set; }

    /// <inheritdoc cref="Declination"/>
    public Declination Declination { get; set; }

    /// <inheritdoc cref="Omnidirect.EquipmentElevation"/>
    public int Elevation { get; set; }

    /// <inheritdoc cref="UsableRange"/>
    public UsableRange Range { get; set; }

    /// <inheritdoc cref="Omnidirect.ProtectionDistance"/>
    public int ProtectionDistance { get; set; }
}
