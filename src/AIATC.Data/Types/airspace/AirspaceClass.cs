namespace AIATC.Data.Models.Types;

/**<summary>
<c>Controlled Airspace Classification (ARSP CLASS)</c> character.
</summary>
<remarks>See section 5.215.</remarks>*/
// ARINC mapping attributes removed
public enum AirspaceClass : byte
{
    Unknown,
    Alpha,
    Bravo,
    Charlie,
    Delta,
    Echo,
    Foxtrot,
    Golf
}
