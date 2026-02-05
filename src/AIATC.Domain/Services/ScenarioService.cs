using AIATC.Domain.Models.Scenarios;
using AIATC.Domain.Models.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for managing scenarios
/// </summary>
public class ScenarioService
{
    private readonly Dictionary<string, Scenario> _scenarios = new();
    private Scenario? _activeScenario;

    /// <summary>
    /// Event raised when scenario state changes
    /// </summary>
    public event EventHandler<ScenarioStateChangedEventArgs>? ScenarioStateChanged;

    /// <summary>
    /// Event raised when an objective is completed
    /// </summary>
    public event EventHandler<ObjectiveCompletedEventArgs>? ObjectiveCompleted;

    /// <summary>
    /// Gets all available scenarios
    /// </summary>
    public IEnumerable<Scenario> GetAllScenarios()
    {
        return _scenarios.Values;
    }

    /// <summary>
    /// Gets a scenario by ID
    /// </summary>
    public Scenario? GetScenario(string scenarioId)
    {
        _scenarios.TryGetValue(scenarioId, out var scenario);
        return scenario;
    }

    /// <summary>
    /// Gets scenarios by difficulty
    /// </summary>
    public IEnumerable<Scenario> GetScenariosByDifficulty(ScenarioDifficulty difficulty)
    {
        return _scenarios.Values
            .Where(s => s.Metadata.Difficulty == difficulty);
    }

    /// <summary>
    /// Gets scenarios by tags
    /// </summary>
    public IEnumerable<Scenario> GetScenariosByTags(params string[] tags)
    {
        return _scenarios.Values
            .Where(s => tags.Any(tag => s.Metadata.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Registers a scenario
    /// </summary>
    public void RegisterScenario(Scenario scenario)
    {
        _scenarios[scenario.Metadata.Id] = scenario;
    }

    /// <summary>
    /// Starts a scenario
    /// </summary>
    public void StartScenario(string scenarioId)
    {
        if (_activeScenario != null && _activeScenario.State == ScenarioState.Running)
        {
            throw new InvalidOperationException("Another scenario is already running");
        }

        var scenario = GetScenario(scenarioId);
        if (scenario == null)
        {
            throw new ArgumentException($"Scenario not found: {scenarioId}");
        }

        scenario.Start();
        _activeScenario = scenario;

        ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
        {
            Scenario = scenario,
            NewState = ScenarioState.Running,
            PreviousState = ScenarioState.NotStarted
        });
    }

    /// <summary>
    /// Gets the currently active scenario
    /// </summary>
    public Scenario? GetActiveScenario()
    {
        return _activeScenario;
    }

    /// <summary>
    /// Updates the active scenario
    /// </summary>
    public void UpdateActiveScenario(float deltaTimeSeconds)
    {
        if (_activeScenario == null || _activeScenario.State != ScenarioState.Running)
            return;

        var previousState = _activeScenario.State;
        _activeScenario.Update(deltaTimeSeconds);

        // Check if state changed
        if (_activeScenario.State != previousState)
        {
            ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
            {
                Scenario = _activeScenario,
                NewState = _activeScenario.State,
                PreviousState = previousState
            });
        }

        // Check for completed objectives
        foreach (var objective in _activeScenario.Objectives.Where(o => !o.IsCompleted))
        {
            if (objective.IsCompleted)
            {
                ObjectiveCompleted?.Invoke(this, new ObjectiveCompletedEventArgs
                {
                    Scenario = _activeScenario,
                    Objective = objective
                });
            }
        }

        // Auto-complete if all required objectives are done
        if (_activeScenario.AreRequiredObjectivesComplete() &&
            _activeScenario.State == ScenarioState.Running)
        {
            CompleteActiveScenario();
        }
    }

    /// <summary>
    /// Pauses the active scenario
    /// </summary>
    public void PauseActiveScenario()
    {
        if (_activeScenario == null)
            return;

        var previousState = _activeScenario.State;
        _activeScenario.Pause();

        if (_activeScenario.State != previousState)
        {
            ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
            {
                Scenario = _activeScenario,
                NewState = _activeScenario.State,
                PreviousState = previousState
            });
        }
    }

    /// <summary>
    /// Resumes the active scenario
    /// </summary>
    public void ResumeActiveScenario()
    {
        if (_activeScenario == null)
            return;

        var previousState = _activeScenario.State;
        _activeScenario.Resume();

        if (_activeScenario.State != previousState)
        {
            ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
            {
                Scenario = _activeScenario,
                NewState = _activeScenario.State,
                PreviousState = previousState
            });
        }
    }

    /// <summary>
    /// Completes the active scenario
    /// </summary>
    public void CompleteActiveScenario()
    {
        if (_activeScenario == null)
            return;

        var previousState = _activeScenario.State;
        _activeScenario.Complete();

        ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
        {
            Scenario = _activeScenario,
            NewState = _activeScenario.State,
            PreviousState = previousState
        });
    }

    /// <summary>
    /// Fails the active scenario
    /// </summary>
    public void FailActiveScenario(string reason)
    {
        if (_activeScenario == null)
            return;

        var previousState = _activeScenario.State;
        _activeScenario.Fail(reason);

        ScenarioStateChanged?.Invoke(this, new ScenarioStateChangedEventArgs
        {
            Scenario = _activeScenario,
            NewState = _activeScenario.State,
            PreviousState = previousState
        });
    }

    /// <summary>
    /// Updates an objective's progress
    /// </summary>
    public void UpdateObjectiveProgress(string objectiveId, float newValue)
    {
        if (_activeScenario == null)
            return;

        var objective = _activeScenario.Objectives.FirstOrDefault(o => o.Id == objectiveId);
        if (objective == null)
            return;

        var wasCompleted = objective.IsCompleted;
        objective.UpdateProgress(newValue);

        if (!wasCompleted && objective.IsCompleted)
        {
            ObjectiveCompleted?.Invoke(this, new ObjectiveCompletedEventArgs
            {
                Scenario = _activeScenario,
                Objective = objective
            });
        }
    }

    /// <summary>
    /// Records an aircraft landing in the active scenario
    /// </summary>
    public void RecordAircraftLanding()
    {
        if (_activeScenario == null)
            return;

        _activeScenario.AircraftLanded++;

        // Update "LandAircraft" objectives
        var landObjectives = _activeScenario.Objectives
            .Where(o => o.Type == ObjectiveType.LandAircraft);

        foreach (var objective in landObjectives)
        {
            UpdateObjectiveProgress(objective.Id, _activeScenario.AircraftLanded);
        }
    }

    /// <summary>
    /// Records a separation violation in the active scenario
    /// </summary>
    public void RecordSeparationViolation()
    {
        if (_activeScenario == null)
            return;

        _activeScenario.SeparationViolations++;

        // Fail "NoViolations" objectives
        var noViolationObjectives = _activeScenario.Objectives
            .Where(o => o.Type == ObjectiveType.NoViolations);

        foreach (var objective in noViolationObjectives)
        {
            objective.IsCompleted = false;
        }
    }

    /// <summary>
    /// Clears all scenarios
    /// </summary>
    public void Clear()
    {
        _scenarios.Clear();
        _activeScenario = null;
    }
}

/// <summary>
/// Event args for scenario state changes
/// </summary>
public class ScenarioStateChangedEventArgs : EventArgs
{
    public Scenario Scenario { get; set; } = null!;
    public ScenarioState NewState { get; set; }
    public ScenarioState PreviousState { get; set; }
}

/// <summary>
/// Event args for objective completion
/// </summary>
public class ObjectiveCompletedEventArgs : EventArgs
{
    public Scenario Scenario { get; set; } = null!;
    public ScenarioObjective Objective { get; set; } = null!;
}
