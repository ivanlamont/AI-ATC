using AIATC.Data.Models.Types;
using AIATC.Data.Models.Ground;

namespace AIATC.Data.Models.Navigation;

/**<summary>
<c>VHF NAVAID</c> primary record.
</summary>
<remarks>See section 4.1.2.1.</remarks>*/
public class Omnidirect : Navaid
{
    public int Id { get; set; }

    public Port? Port { get; set; }

    public OmnidirectType Type { get; set; }
    public OmnidirectCoverage Coverage { get; set; }
    public OmnidirectInfo Info { get; set; }
    public OmnidirectCollocation Collocation { get; set; }

    /**<summary>
    <c>DME Identifier (DME IDENT)</c> field.
    </summary>
    <remarks>See section 5.38.</remarks>*/
    public string? EquipmentIdentifier { get; set; }

    public Coordinates? EquipmentCoordinates { get; set; }

    public Declination Declination { get; set; }

    /**<summary>
    <c>DME Elevation (DME ELEV)</c> field.
    </summary>
    <remarks>See section 5.40.</remarks>*/
    public int EquipmentElevation { get; set; }

    public UsableRange Range { get; set; }

    /**<summary>
    <c>ILS/DME Bias</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.90.</remarks>*/
    public float EquipmentOffset { get; set; }

    /**<summary>
    <c>Frequency Protection Distance (FREQ PRD)</c> field.
    </summary>
    <value>Nautical miles.</value>
    <remarks>See section 5.150.</remarks>*/
    public int ProtectionDistance { get; set; }

    /**<summary>
    <c>Route Inappropriate Navaid Indicator</c> character.
    </summary>
    <remarks>See section 5.297.</remarks>*/
    public Bool NotAreaNavigation { get; set; }

    public ServiceVolume ServiceVolume { get; set; }
}
