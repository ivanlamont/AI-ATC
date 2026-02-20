namespace AIATC.Data.Models;

/**<summary>
Basic <c>ARINC-424</c> record with area/customer code,
file record number and cycle date fields.
</summary>*/
public abstract class Record424
{
    /// <summary>Primary key for database storage.</summary>
    public int Id { get; set; }

    /// <summary>The source string from which the record was created.</summary>
    public string? Source { get; set; }

    /**<summary>
    <c>Customer/Area Code (CUST/AREA)</c> field.
    </summary>
    <remarks>See section 5.3.</remarks>*/
    public string? Code { get; set; }

    /**<summary>
    <c>File Record Number (FRN)</c> field.
    </summary>
    <remarks>See section 5.31.</remarks>*/
    public int Number { get; set; }

    /**<summary>
    <c>Cycle Date</c> field.
    </summary>
    <remarks>See section 5.32</remarks>*/
    public int Date { get; set; }
}
