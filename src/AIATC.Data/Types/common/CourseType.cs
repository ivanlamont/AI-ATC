namespace AIATC.Data.Types.Common;

/**<summary>
<c>Magnetic/True Indicator (M/T IND)</c> character.
</summary>
<remarks>See section 5.165.</remarks>*/
// ARINC mapping attributes removed
public enum CourseType : byte
{
    Unknown = 0,
    Magnetic = 1,
    True = 1 << 1
}
