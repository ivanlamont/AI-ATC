namespace AIATC.Data.Models.Types;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a radio frequency
/// </summary>
[Owned]
public class Frequency
{
    public double Value { get; set; }
    public string Unit { get; set; } = "MHZ";

    public Frequency()
    {
    }

    public Frequency(double value, string unit = "MHZ")
    {
        Value = value;
        Unit = unit;
    }
}
