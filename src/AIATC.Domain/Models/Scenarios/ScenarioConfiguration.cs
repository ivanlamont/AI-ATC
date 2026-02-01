using AIATC.Domain.Models.Weather;
using AIATC.Domain.Services;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Scenarios;

/// <summary>
/// Configuration parameters for a scenario
/// </summary>
public class ScenarioConfiguration
{
    /// <summary>
    /// Aircraft spawn configuration
    /// </summary>
    public AircraftSpawnConfig AircraftConfig { get; set; } = new();

    /// <summary>
    /// Weather configuration
    /// </summary>
    public WeatherConfig WeatherConfig { get; set; } = new();

    /// <summary>
    /// Airspace configuration
    /// </summary>
    public AirspaceConfig AirspaceConfig { get; set; } = new();

    /// <summary>
    /// Simulation parameters
    /// </summary>
    public SimulationConfig SimulationConfig { get; set; } = new();

    /// <summary>
    /// Scoring parameters
    /// </summary>
    public ScoringConfig ScoringConfig { get; set; } = new();
}

/// <summary>
/// Aircraft spawning configuration
/// </summary>
public class AircraftSpawnConfig
{
    /// <summary>
    /// Initial number of aircraft
    /// </summary>
    public int InitialAircraftCount { get; set; } = 3;

    /// <summary>
    /// Maximum concurrent aircraft
    /// </summary>
    public int MaximumAircraftCount { get; set; } = 10;

    /// <summary>
    /// Aircraft spawn rate (aircraft per minute)
    /// </summary>
    public float SpawnRatePerMinute { get; set; } = 1.0f;

    /// <summary>
    /// Percentage of arrivals (0-100, remainder are departures)
    /// </summary>
    public float ArrivalPercentage { get; set; } = 70f;

    /// <summary>
    /// Allowed aircraft types
    /// </summary>
    public List<string> AllowedAircraftTypes { get; set; } = new() { "B738", "A320", "B77W", "E75L" };

    /// <summary>
    /// Minimum spawn distance from airport (NM)
    /// </summary>
    public float MinSpawnDistanceNm { get; set; } = 30f;

    /// <summary>
    /// Maximum spawn distance from airport (NM)
    /// </summary>
    public float MaxSpawnDistanceNm { get; set; } = 60f;

    /// <summary>
    /// Spawn altitude range (feet MSL)
    /// </summary>
    public (float Min, float Max) SpawnAltitudeRange { get; set; } = (3000f, 12000f);

    /// <summary>
    /// Whether to use random callsigns
    /// </summary>
    public bool UseRandomCallsigns { get; set; } = true;
}

/// <summary>
/// Weather configuration
/// </summary>
public class WeatherConfig
{
    /// <summary>
    /// Weather difficulty level
    /// </summary>
    public WeatherDifficulty Difficulty { get; set; } = WeatherDifficulty.Easy;

    /// <summary>
    /// Fixed weather conditions (null for random)
    /// </summary>
    public WeatherConditions? FixedWeather { get; set; }

    /// <summary>
    /// Whether weather evolves over time
    /// </summary>
    public bool DynamicWeather { get; set; } = false;

    /// <summary>
    /// Weather update interval (seconds)
    /// </summary>
    public float UpdateIntervalSeconds { get; set; } = 30f;
}

/// <summary>
/// Airspace configuration
/// </summary>
public class AirspaceConfig
{
    /// <summary>
    /// Active sectors
    /// </summary>
    public List<string> ActiveSectors { get; set; } = new();

    /// <summary>
    /// Controlled airspace radius (NM)
    /// </summary>
    public float ControlledAirspaceRadiusNm { get; set; } = 60f;

    /// <summary>
    /// Minimum separation (NM)
    /// </summary>
    public float MinimumSeparationNm { get; set; } = 3f;

    /// <summary>
    /// Vertical separation (feet)
    /// </summary>
    public float VerticalSeparationFt { get; set; } = 1000f;

    /// <summary>
    /// Whether handoffs are required
    /// </summary>
    public bool HandoffsEnabled { get; set; } = false;
}

/// <summary>
/// Simulation configuration
/// </summary>
public class SimulationConfig
{
    /// <summary>
    /// Simulation time scale (1.0 = real-time, 2.0 = 2x speed)
    /// </summary>
    public float TimeScale { get; set; } = 1.0f;

    /// <summary>
    /// Pause simulation at start
    /// </summary>
    public bool StartPaused { get; set; } = false;

    /// <summary>
    /// Enable AI assistance
    /// </summary>
    public bool AiAssistanceEnabled { get; set; } = false;

    /// <summary>
    /// Collision avoidance system
    /// </summary>
    public bool CollisionAvoidanceEnabled { get; set; } = true;

    /// <summary>
    /// Show warnings for separation violations
    /// </summary>
    public bool ShowWarnings { get; set; } = true;
}

/// <summary>
/// Scoring configuration
/// </summary>
public class ScoringConfig
{
    /// <summary>
    /// Enable scoring
    /// </summary>
    public bool ScoringEnabled { get; set; } = true;

    /// <summary>
    /// Difficulty multiplier for score
    /// </summary>
    public float DifficultyMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Target score for scenario completion
    /// </summary>
    public int TargetScore { get; set; } = 1000;

    /// <summary>
    /// Deduct points for separation violations
    /// </summary>
    public bool PenalizeSeparationViolations { get; set; } = true;

    /// <summary>
    /// Bonus points for efficient routing
    /// </summary>
    public bool BonusForEfficiency { get; set; } = true;
}
