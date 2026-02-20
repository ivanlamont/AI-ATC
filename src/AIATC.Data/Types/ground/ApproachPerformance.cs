namespace AIATC.Data.Models.Types;

using T = ApproachPerformance;

/**<summary>
<c>Approach Performance Designator (APD)</c> character.
</summary>
<remarks>See section 5.258.</remarks>*/
// ...existing code...
public enum ApproachPerformance : byte
{
    Unknown = 0,
    /**<summary>
    GAST A.
    </summary>*/
    // ...existing code...
    Alpha = 1,
    /**<summary>
    GAST B.
    </summary>*/
    Bravo = 1 << 1,
    /**<summary>
    GAST C.
    </summary>*/
    // ...existing code...
    Charlie = 1 << 2,
    /**<summary>
    GAST D.
    </summary>*/
    Delta = 1 << 3
}
