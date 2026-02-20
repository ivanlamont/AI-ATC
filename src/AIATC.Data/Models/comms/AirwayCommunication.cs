using AIATC.Data.Models.Types;

namespace AIATC.Data.Models.Comms;

/**<summary>
<c>Enroute Communications</c> primary record sequence.
</summary>
<remarks>See section 4.1.23.1.</remarks>*/
public class AirwayCommunication : Communication<AirwayTransmitter>
{
    public int Id { get; set; }

    /// <summary><c>FIR/RDO Identifier (FIR/RDO)</c> field.</summary>
    /// <remarks>See section 5.190.</remarks>
    public string Identifier { get; set; }

    /// <summary><c>FIR/UIR Address (ADDRESS)</c> field.</summary>
    /// <remarks>See section 5.151.</remarks>
    public string? Address { get; set; }

    /// <inheritdoc cref="RegionType"/>
    public RegionType Type { get; set; }
}
