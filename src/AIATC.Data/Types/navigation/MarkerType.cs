namespace AIATC.Data.Models.Types;

/// <summary>
/// Marker Type (MKR TYPE) field.
/// </summary>
[System.Flags]
public enum MarkerType : byte
{
    Unknown = 0,
    Locator = 1,
    Inner = 1 << 1,
    Middle = 1 << 2,
    Outer = 1 << 3,
    Back = 1 << 4,
}
