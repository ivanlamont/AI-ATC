using System.Globalization;

namespace AIATC.ScenarioService.Services;

internal static class ArincCoordinateParser
{
    public static bool TryParseLatitude(string? value, out double result) =>
        TryParseCoordinate(value, expectedLatitude: true, out result);

    public static bool TryParseLongitude(string? value, out double result) =>
        TryParseCoordinate(value, expectedLatitude: false, out result);

    private static bool TryParseCoordinate(string? value, bool expectedLatitude, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim().ToUpperInvariant();

        // Support plain decimal degrees as fallback.
        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        char hemisphere;
        string body;

        if (trimmed.Length > 1 && IsHemisphere(trimmed[0]))
        {
            hemisphere = trimmed[0];
            body = trimmed[1..];
        }
        else if (trimmed.Length > 1 && IsHemisphere(trimmed[^1]))
        {
            hemisphere = trimmed[^1];
            body = trimmed[..^1];
        }
        else
        {
            return false;
        }

        if (expectedLatitude && hemisphere is not ('N' or 'S'))
        {
            return false;
        }

        if (!expectedLatitude && hemisphere is not ('E' or 'W'))
        {
            return false;
        }

        var digits = new string(body.Where(char.IsDigit).ToArray());
        var degreeDigits = expectedLatitude ? 2 : 3;

        if (digits.Length < degreeDigits + 4)
        {
            return false;
        }

        if (!int.TryParse(digits[..degreeDigits], NumberStyles.Integer, CultureInfo.InvariantCulture, out var degrees) ||
            !int.TryParse(digits.Substring(degreeDigits, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) ||
            !int.TryParse(digits.Substring(degreeDigits + 2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var wholeSeconds))
        {
            return false;
        }

        var fractionalSeconds = 0d;
        var frac = digits[(degreeDigits + 4)..];
        if (frac.Length > 0)
        {
            fractionalSeconds = int.Parse(frac, CultureInfo.InvariantCulture) / Math.Pow(10, frac.Length);
        }

        var seconds = wholeSeconds + fractionalSeconds;
        var decimalDegrees = degrees + (minutes / 60d) + (seconds / 3600d);
        if (hemisphere is 'S' or 'W')
        {
            decimalDegrees = -decimalDegrees;
        }

        result = decimalDegrees;
        return true;
    }

    private static bool IsHemisphere(char c) => c is 'N' or 'S' or 'E' or 'W';
}
