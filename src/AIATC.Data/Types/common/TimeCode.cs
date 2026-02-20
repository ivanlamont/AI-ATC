namespace AIATC;

/**<summary>
<c>Time Code (TIME CD)</c> character.
</summary>
<remarks>See section 5.131.</remarks>*/
// ...existing code...
public enum TimeCode : byte
{
    Unknown,
    /**<summary>
    Active continuously, including holidays.
    </summary>*/
    WithHolidays,
    /**<summary>
    Active continuously, excluding holidays.
    </summary>*/
    WithoutHolidays,
    /**<summary>
    Active non-continuously, refer to Continuation Record.
    </summary>*/
    NonContinuously,
    /**<summary>
    Active times announced by NOTAM.
    </summary>*/
    ByNotam,
    /**<summary>
    Active times are not specified in source documentation.
    </summary>*/
    Unspecified
}
