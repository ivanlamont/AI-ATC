namespace AIATC.Data.Models.Types;

/**<summary>
<c>SBAS Service Provider Identifier (SBAS ID)</c> field.
</summary>
<remarks>See section 5.255.</remarks>*/
// ARINC mapping attributes removed
public enum SatelliteService : byte
{
    Unknown,
    /**<summary>
    Not intended for SBAS.
    </summary>*/
    NotIntended,
    /**<summary>
    Any Service provider may be used.
    </summary>*/
    Any,
    /**<summary>
    WAAS.
    </summary>*/
    Waas,
    /**<summary>
    EGNOS.
    </summary>*/
    Egnos,
    /**<summary>
    MSAS.
    </summary>*/
    Msas,
    /**<summary>
    GAGAN.
    </summary>*/
    Gagan,
    /**<summary>
    SDCM.
    </summary>*/
    Sdcm,
}
