using AIATC.Domain.Models;
using AIATC.Domain.Models.Scenarios;
using AIATC.Domain.Models.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Services;

/// <summary>
/// Main simulation engine that coordinates all simulation systems
/// </summary>
public class SimulationEngine
{
    private readonly TimeController _timeController;
    private readonly ScenarioService _scenarioService;
    private readonly WeatherService _weatherService;
    private readonly List<AircraftModel> _aircraft = new();

    /// <summary>
    /// Current simulation time (accumulated scaled time)
    /// </summary>
    public float SimulationTimeSeconds { get; private set; }

    /// <summary>
    /// Total real-world elapsed time
    /// </summary>
    public float RealTimeSeconds { get; private set; }

    /// <summary>
    /// Time controller for managing time scale
    /// </summary>
    public TimeController TimeController => _timeController;

    /// <summary>
    /// Scenario service
    /// </summary>
    public ScenarioService ScenarioService => _scenarioService;

    /// <summary>
    /// Weather service
    /// </summary>
    public WeatherService WeatherService => _weatherService;

    /// <summary>
    /// All active aircraft
    /// </summary>
    public IReadOnlyList<AircraftModel> Aircraft => _aircraft.AsReadOnly();

    /// <summary>
    /// Current score (from active scenario)
    /// </summary>
    public int CurrentScore => _scenarioService.GetActiveScenario()?.CurrentScore ?? 0;

    /// <summary>
    /// Event raised when an aircraft lands
    /// </summary>
    public event EventHandler<AircraftLandedEventArgs>? AircraftLanded;

    public SimulationEngine(
        TimeController? timeController = null,
        ScenarioService? scenarioService = null,
        WeatherService? weatherService = null)
    {
        _timeController = timeController ?? new TimeController();
        _scenarioService = scenarioService ?? new ScenarioService();
        _weatherService = weatherService ?? new WeatherService();
    }

    /// <summary>
    /// Updates the simulation by one frame
    /// </summary>
    public void Update(float deltaTimeSeconds)
    {
        // Track real time
        RealTimeSeconds += deltaTimeSeconds;

        // Apply time scale
        var scaledDeltaTime = _timeController.ApplyTimeScale(deltaTimeSeconds);

        // Track simulation time
        SimulationTimeSeconds += scaledDeltaTime;

        // Update active scenario
        _scenarioService.UpdateActiveScenario(scaledDeltaTime);

        // Update aircraft
        UpdateAircraft(scaledDeltaTime);

        // Update weather (less frequent)
        if ((int)SimulationTimeSeconds % 30 == 0) // Every 30 sim seconds
        {
            foreach (var location in _weatherService.GetLocations())
            {
                _weatherService.UpdateWeather(location, scaledDeltaTime);
            }
        }
    }

    private void UpdateAircraft(float scaledDeltaTime)
    {
        foreach (var aircraft in _aircraft.ToList())
        {
            // Apply weather to aircraft
            var scenario = _scenarioService.GetActiveScenario();
            if (scenario != null)
            {
                var weather = _weatherService.GetWeather(scenario.Metadata.LocationId);
                weather.ApplyToAircraft(aircraft);
            }

            // Update aircraft physics
            aircraft.Step(scaledDeltaTime);

            // Check for landing
            if (aircraft.Destination != null && !aircraft.Landed)
            {
                if (aircraft.CheckLanding(aircraft.Destination, 0.5f))
                {
                    OnAircraftLanded(aircraft);
                }
            }
        }
    }

    private void OnAircraftLanded(AircraftModel aircraft)
    {
        // Record in scenario
        _scenarioService.RecordAircraftLanding();

        // Calculate score bonus with time multiplier
        var scenario = _scenarioService.GetActiveScenario();
        if (scenario != null)
        {
            var basePoints = 100;
            var multiplier = _timeController.GetScoreMultiplier(
                scenario.Configuration.ScoringConfig.DifficultyMultiplier);

            var points = (int)(basePoints * multiplier);
            scenario.CurrentScore += points;
        }

        // Raise event
        AircraftLanded?.Invoke(this, new AircraftLandedEventArgs
        {
            Aircraft = aircraft
        });
    }

    /// <summary>
    /// Adds an aircraft to the simulation
    /// </summary>
    public void AddAircraft(AircraftModel aircraft)
    {
        _aircraft.Add(aircraft);

        var scenario = _scenarioService.GetActiveScenario();
        if (scenario != null)
        {
            scenario.AircraftSpawned++;
        }
    }

    /// <summary>
    /// Removes an aircraft from the simulation
    /// </summary>
    public void RemoveAircraft(AircraftModel aircraft)
    {
        _aircraft.Remove(aircraft);
    }

    /// <summary>
    /// Clears all aircraft
    /// </summary>
    public void ClearAircraft()
    {
        _aircraft.Clear();
    }

    /// <summary>
    /// Resets the simulation
    /// </summary>
    public void Reset()
    {
        SimulationTimeSeconds = 0;
        RealTimeSeconds = 0;
        _aircraft.Clear();
        _timeController.Reset();
    }

    /// <summary>
    /// Gets the effective score multiplier (combines difficulty and time scale)
    /// </summary>
    public float GetEffectiveScoreMultiplier()
    {
        var scenario = _scenarioService.GetActiveScenario();
        var baseMultiplier = scenario?.Configuration.ScoringConfig.DifficultyMultiplier ?? 1.0f;
        return _timeController.GetScoreMultiplier(baseMultiplier);
    }
}

/// <summary>
/// Event args for aircraft landing
/// </summary>
public class AircraftLandedEventArgs : EventArgs
{
    public AircraftModel Aircraft { get; set; } = null!;
}
