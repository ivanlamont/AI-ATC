namespace AIATC.Data.Models.Types;

/**<summary>
<c>Controlled Airspace Type (ARSP TYPE)</c> character.
</summary>
<remarks>See section 5.213.</remarks>*/
// ARINC mapping attributes removed
public enum AirspaceType : byte
{
    Unknown,
    /**<summary>
    Class C Airspace.
    </summary>*/
    Charlie,
    ControlArea,
    TerminalControlArea,
    RadarZone,
    Bravo,
    RadioMandatory,
    TransponderMandatory,
    ControlZone
}
