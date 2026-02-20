namespace AIATC.Data.Models.Types;

/**<summary>
<c>Restrictive Airspace Type (REST TYPE)</c> character.
</summary>
<remarks>See section 5.128.</remarks>*/
// ...existing code...
public enum RestrictiveType : byte
{
    Unknown,
    Alert,
    Caution,
    Danger,
    LongTerm,
    MilitaryOperations,
    NationalSecurity,
    Prohibited,
    Restricted,
    Training,
    Warning,
    Unspecified
}
