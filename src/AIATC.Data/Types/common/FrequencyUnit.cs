namespace AIATC;

/**<summary>
<c>Frequency Units (FREQ UNIT)</c> character.
</summary>
<remarks>See section 5.104.</remarks>*/
// ...existing code...
public enum FrequencyUnit : byte
{
    Unknown,
    /**<summary>
    Low Frequency.
    </summary>*/
    Low,
    /**<summary>
    Medium Frequency.
    </summary>*/
    Medium,
    /**<summary>
    High Frequency (2000 kHz to 30,000 kHz).
    </summary>*/
    High,
    /**<summary>
    Very High Frequency 100 kHz spacing.
    </summary>*/
    VeryHighSpacing100,
    /**<summary>
    Very High Frequency 50 kHz spacing.
    </summary>*/
    VeryHighSpacing50,
    /**<summary>
    Very High Frequency 25 kHz spacing.
    </summary>*/
    VeryHighSpacing25,
    /**<summary>
    Very High Frequency (30,000 kHz to 200 MHz) Non-standard spacing.
    </summary>*/
    VeryHighNonStandardSpacing,
    UltraHigh,
    Channel,
    Digital
}
