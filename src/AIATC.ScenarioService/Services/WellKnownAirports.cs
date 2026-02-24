namespace AIATC.ScenarioService.Services;

/// <summary>
/// Hardcoded fallback airport data for common airports used when the ARINC 424
/// reference database (AirspaceDb) is not provisioned in the current environment.
/// </summary>
internal static class WellKnownAirports
{
    internal record AirportInfo(
        string IcaoCode,
        string Name,
        double Latitude,
        double Longitude,
        int ElevationFt,
        IReadOnlyList<RunwayInfo> Runways);

    internal record RunwayInfo(
        string Identifier,
        double LengthFt,
        double WidthFt,
        double Heading,
        double Latitude,
        double Longitude);

    private static readonly Dictionary<string, AirportInfo> _airports =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["KSFO"] = new("KSFO", "San Francisco International", 37.6213, -122.3790, 13,
        [
            new("10L/28R", 11870, 200, 100.0, 37.6196, -122.3578),
            new("10R/28L", 11381, 200, 100.0, 37.6148, -122.3539),
            new("01L/19R", 8651,  200,  10.0, 37.6063, -122.3900),
            new("01R/19L", 7650,  200,  10.0, 37.6068, -122.3852),
        ]),

        ["KATL"] = new("KATL", "Hartsfield-Jackson Atlanta International", 33.6407, -84.4277, 1026,
        [
            new("08L/26R", 9000,  150,  80.0, 33.6392, -84.4510),
            new("08R/26L", 9000,  150,  80.0, 33.6340, -84.4510),
            new("09L/27R", 11889, 150,  90.0, 33.6440, -84.4510),
            new("09R/27L", 9000,  150,  90.0, 33.6492, -84.4510),
            new("10/28",   9000,  150, 100.0, 33.6542, -84.4510),
        ]),

        ["KLAX"] = new("KLAX", "Los Angeles International", 33.9425, -118.4081, 125,
        [
            new("06L/24R", 8926,  150, 69.0, 33.9373, -118.4260),
            new("06R/24L", 11096, 150, 69.0, 33.9461, -118.4260),
            new("07L/25R", 12091, 150, 69.0, 33.9380, -118.4260),
            new("07R/25L", 11095, 150, 69.0, 33.9449, -118.4260),
        ]),

        ["KJFK"] = new("KJFK", "John F. Kennedy International", 40.6413, -73.7781, 13,
        [
            new("04L/22R", 8400,  150,  40.0, 40.6293, -73.7823),
            new("04R/22L", 11351, 150,  40.0, 40.6184, -73.7648),
            new("13L/31R", 10000, 150, 130.0, 40.6522, -73.8003),
            new("13R/31L", 14511, 150, 130.0, 40.6611, -73.7949),
        ]),

        ["KORD"] = new("KORD", "Chicago O'Hare International", 41.9742, -87.9073, 672,
        [
            new("04L/22R", 7500,  150,  40.0, 41.9660, -87.9210),
            new("04R/22L", 13000, 150,  40.0, 41.9870, -87.9250),
            new("09L/27R", 10000, 150,  90.0, 41.9750, -87.9320),
            new("09R/27L", 7500,  150,  90.0, 41.9695, -87.9320),
            new("10C/28C", 13000, 150, 100.0, 41.9710, -87.9330),
            new("10L/28R", 13000, 150, 100.0, 41.9780, -87.9330),
            new("10R/28L", 7500,  150, 100.0, 41.9638, -87.9330),
        ]),

        ["KDFW"] = new("KDFW", "Dallas/Fort Worth International", 32.8998, -97.0403, 603,
        [
            new("17L/35R", 13401, 150, 170.0, 32.9200, -97.0410),
            new("17C/35C", 13400, 150, 170.0, 32.9200, -97.0350),
            new("17R/35L", 13401, 150, 170.0, 32.9200, -97.0290),
            new("18L/36R", 9000,  150, 180.0, 32.9150, -97.0190),
            new("18R/36L", 13400, 150, 180.0, 32.9150, -97.0530),
        ]),

        ["KDEN"] = new("KDEN", "Denver International", 39.8561, -104.6737, 5431,
        [
            new("07/25",   11002, 150,  70.0, 39.8570, -104.7000),
            new("08/26",   12000, 150,  80.0, 39.8490, -104.7000),
            new("16L/34R", 12000, 150, 160.0, 39.8750, -104.6900),
            new("16R/34L", 16000, 150, 160.0, 39.8750, -104.6700),
            new("17L/35R", 12000, 150, 170.0, 39.8750, -104.6600),
            new("17R/35L", 12000, 150, 170.0, 39.8750, -104.6550),
        ]),

        ["KSEA"] = new("KSEA", "Seattle-Tacoma International", 47.4502, -122.3088, 433,
        [
            new("16L/34R", 11900, 150, 160.0, 47.4643, -122.3099),
            new("16C/34C", 11900, 150, 160.0, 47.4643, -122.3010),
        ]),

        ["KLAS"] = new("KLAS", "Harry Reid International", 36.0840, -115.1537, 2141,
        [
            new("01L/19R", 9000,  150,  10.0, 36.0723, -115.1650),
            new("01R/19L", 10000, 150,  10.0, 36.0723, -115.1590),
            new("07L/25R", 14510, 150,  70.0, 36.0839, -115.1730),
            new("07R/25L", 9000,  150,  70.0, 36.0790, -115.1730),
        ]),

        ["KMIA"] = new("KMIA", "Miami International", 25.7959, -80.2870, 8,
        [
            new("08L/26R", 13000, 150,  80.0, 25.7950, -80.3130),
            new("08R/26L", 10500, 150,  80.0, 25.7900, -80.3130),
            new("09/27",   9354,  150,  90.0, 25.7970, -80.3160),
            new("12/30",   8600,  150, 120.0, 25.8020, -80.3040),
        ]),

        ["KBOS"] = new("KBOS", "Boston Logan International", 42.3656, -71.0096, 19,
        [
            new("04L/22R", 10005, 150,  40.0, 42.3545, -71.0195),
            new("04R/22L", 7000,  150,  40.0, 42.3618, -71.0165),
            new("09/27",   7001,  150,  90.0, 42.3631, -71.0280),
            new("15R/33L", 10081, 150, 150.0, 42.3792, -70.9930),
        ]),

        ["KPHX"] = new("KPHX", "Phoenix Sky Harbor International", 33.4373, -112.0078, 1135,
        [
            new("07L/25R", 11489, 150, 70.0, 33.4310, -112.0360),
            new("07R/25L", 7800,  150, 70.0, 33.4380, -112.0360),
            new("08/26",   7202,  150, 80.0, 33.4432, -112.0360),
        ]),
    };

    public static AirportInfo? TryGet(string icaoCode)
    {
        var key = AirportReferenceLookup.Normalize(icaoCode);
        return _airports.TryGetValue(key, out var info) ? info : null;
    }
}
