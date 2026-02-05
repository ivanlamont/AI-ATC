using System;

namespace AIATC.Domain.Services;

/// <summary>
/// Controls simulation time scale and provides score multipliers
/// </summary>
public class TimeController
{
    private float _timeScale = 1.0f;
    private bool _isPaused = false;

    /// <summary>
    /// Minimum allowed time scale
    /// </summary>
    public float MinTimeScale { get; set; } = 0.1f;

    /// <summary>
    /// Maximum allowed time scale
    /// </summary>
    public float MaxTimeScale { get; set; } = 5.0f;

    /// <summary>
    /// Current time scale (1.0 = real-time)
    /// </summary>
    public float TimeScale
    {
        get => _timeScale;
        set
        {
            var oldValue = _timeScale;
            _timeScale = Math.Clamp(value, MinTimeScale, MaxTimeScale);

            if (Math.Abs(_timeScale - oldValue) > 0.001f)
            {
                TimeScaleChanged?.Invoke(this, new TimeScaleChangedEventArgs
                {
                    OldTimeScale = oldValue,
                    NewTimeScale = _timeScale
                });
            }
        }
    }

    /// <summary>
    /// Whether simulation is paused
    /// </summary>
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (_isPaused != value)
            {
                _isPaused = value;
                PauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs
                {
                    IsPaused = _isPaused
                });
            }
        }
    }

    /// <summary>
    /// Gets effective time scale (0 if paused)
    /// </summary>
    public float EffectiveTimeScale => _isPaused ? 0f : _timeScale;

    /// <summary>
    /// Event raised when time scale changes
    /// </summary>
    public event EventHandler<TimeScaleChangedEventArgs>? TimeScaleChanged;

    /// <summary>
    /// Event raised when pause state changes
    /// </summary>
    public event EventHandler<PauseStateChangedEventArgs>? PauseStateChanged;

    /// <summary>
    /// Applies time scale to delta time
    /// </summary>
    public float ApplyTimeScale(float deltaTimeSeconds)
    {
        return deltaTimeSeconds * EffectiveTimeScale;
    }

    /// <summary>
    /// Gets score multiplier based on current time scale
    /// </summary>
    /// <param name="baseMultiplier">Base multiplier (e.g., from difficulty)</param>
    public float GetScoreMultiplier(float baseMultiplier = 1.0f)
    {
        // Linear scaling: faster time = higher multiplier
        // 1x speed = 1.0x multiplier
        // 2x speed = 1.5x multiplier
        // 3x speed = 2.0x multiplier
        // 4x speed = 2.5x multiplier
        // 5x speed = 3.0x multiplier
        var timeMultiplier = 1.0f + (_timeScale - 1.0f) * 0.5f;
        return baseMultiplier * timeMultiplier;
    }

    /// <summary>
    /// Gets score multiplier with diminishing returns for very high speeds
    /// </summary>
    public float GetScoreMultiplierWithDiminishingReturns(float baseMultiplier = 1.0f)
    {
        // Logarithmic scaling for diminishing returns
        // 1x = 1.0x, 2x = 1.35x, 3x = 1.58x, 4x = 1.75x, 5x = 1.89x
        var timeMultiplier = 1.0f + MathF.Log(_timeScale + 0.5f, 2.0f) * 0.5f;
        return baseMultiplier * timeMultiplier;
    }

    /// <summary>
    /// Sets time scale to predefined values
    /// </summary>
    public void SetTimeScalePreset(TimeScalePreset preset)
    {
        TimeScale = preset switch
        {
            TimeScalePreset.Paused => 0f,
            TimeScalePreset.Quarter => 0.25f,
            TimeScalePreset.Half => 0.5f,
            TimeScalePreset.Normal => 1.0f,
            TimeScalePreset.Double => 2.0f,
            TimeScalePreset.Triple => 3.0f,
            TimeScalePreset.Quadruple => 4.0f,
            TimeScalePreset.Quintuple => 5.0f,
            _ => 1.0f
        };

        // Handle pause separately
        if (preset == TimeScalePreset.Paused)
        {
            IsPaused = true;
        }
    }

    /// <summary>
    /// Increases time scale by a step
    /// </summary>
    public void IncreaseTimeScale(float step = 0.5f)
    {
        TimeScale += step;
    }

    /// <summary>
    /// Decreases time scale by a step
    /// </summary>
    public void DecreaseTimeScale(float step = 0.5f)
    {
        TimeScale -= step;
    }

    /// <summary>
    /// Toggles pause state
    /// </summary>
    public void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    /// <summary>
    /// Resets to normal speed
    /// </summary>
    public void Reset()
    {
        TimeScale = 1.0f;
        IsPaused = false;
    }

    /// <summary>
    /// Gets real-world time for a given simulation time
    /// </summary>
    public TimeSpan GetRealTimeForSimulationTime(TimeSpan simulationTime)
    {
        if (_timeScale == 0)
            return TimeSpan.MaxValue;

        return TimeSpan.FromSeconds(simulationTime.TotalSeconds / _timeScale);
    }

    /// <summary>
    /// Gets simulation time for a given real-world time
    /// </summary>
    public TimeSpan GetSimulationTimeForRealTime(TimeSpan realTime)
    {
        return TimeSpan.FromSeconds(realTime.TotalSeconds * _timeScale);
    }

    /// <summary>
    /// Checks if time scale is at normal speed
    /// </summary>
    public bool IsNormalSpeed => Math.Abs(_timeScale - 1.0f) < 0.001f;

    /// <summary>
    /// Checks if time is running faster than normal
    /// </summary>
    public bool IsFasterThanNormal => _timeScale > 1.0f;

    /// <summary>
    /// Checks if time is running slower than normal
    /// </summary>
    public bool IsSlowerThanNormal => _timeScale < 1.0f;
}

/// <summary>
/// Predefined time scale presets
/// </summary>
public enum TimeScalePreset
{
    Paused,
    Quarter,    // 0.25x
    Half,       // 0.5x
    Normal,     // 1.0x
    Double,     // 2.0x
    Triple,     // 3.0x
    Quadruple,  // 4.0x
    Quintuple   // 5.0x
}

/// <summary>
/// Event args for time scale changes
/// </summary>
public class TimeScaleChangedEventArgs : EventArgs
{
    public float OldTimeScale { get; set; }
    public float NewTimeScale { get; set; }
}

/// <summary>
/// Event args for pause state changes
/// </summary>
public class PauseStateChangedEventArgs : EventArgs
{
    public bool IsPaused { get; set; }
}
