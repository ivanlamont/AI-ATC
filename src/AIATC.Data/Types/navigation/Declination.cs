namespace AIATC.Data.Models.Types;

/// <summary>
/// Station Declination (STN DEC) field.
/// </summary>
public readonly struct Declination
{
    public float Value { get; }
    public DeclinationType Type { get; }

    public Declination(float value, DeclinationType type)
    {
        Value = value;
        Type = type;
    }
}
