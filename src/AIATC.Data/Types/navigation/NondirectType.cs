namespace AIATC.Data.Models.Types;

/// <summary>
/// First two characters of NAVAID Class (CLASS) field, specific to Navigation.Nondirect.
/// </summary>
[System.Flags]
public enum NondirectType : byte
{
    Unknown = 0,
    Nondirect = 1,
    WithWeather = 1 << 1,
    Marine = 1 << 2,
    Inner = 1 << 3,
    Middle = 1 << 4,
    Outer = 1 << 5,
    Back = 1 << 6
}
