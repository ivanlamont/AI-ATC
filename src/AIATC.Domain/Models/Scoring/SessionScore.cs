using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Models.Scoring;

/// <summary>
/// Tracks the score for an ATC session
/// </summary>
public class SessionScore
{
    /// <summary>
    /// Session identifier
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session end time (null if still active)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Current total score
    /// </summary>
    public int TotalScore { get; set; }

    /// <summary>
    /// Base score (before multipliers)
    /// </summary>
    public int BaseScore { get; set; }

    /// <summary>
    /// Time acceleration multiplier
    /// </summary>
    public float TimeMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// All scoring events in this session
    /// </summary>
    public List<ScoreEvent> Events { get; set; } = new();

    /// <summary>
    /// Statistics for this session
    /// </summary>
    public SessionStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Adds a scoring event and updates the total score
    /// </summary>
    public void AddEvent(ScoreEvent scoreEvent)
    {
        Events.Add(scoreEvent);
        BaseScore += scoreEvent.Points;
        TotalScore = (int)(BaseScore * TimeMultiplier);

        // Update statistics
        UpdateStatistics(scoreEvent);
    }

    /// <summary>
    /// Gets events by type
    /// </summary>
    public List<ScoreEvent> GetEventsByType(ScoreEventType type)
    {
        return Events.Where(e => e.Type == type).ToList();
    }

    /// <summary>
    /// Gets all violations
    /// </summary>
    public List<ScoreEvent> GetViolations()
    {
        return Events.Where(e => e.Severity != SeverityLevel.None).ToList();
    }

    /// <summary>
    /// Calculates session duration
    /// </summary>
    public TimeSpan GetDuration()
    {
        var end = EndTime ?? DateTime.UtcNow;
        return end - StartTime;
    }

    /// <summary>
    /// Gets points per minute
    /// </summary>
    public float GetPointsPerMinute()
    {
        var duration = GetDuration().TotalMinutes;
        return duration > 0 ? TotalScore / (float)duration : 0;
    }

    private void UpdateStatistics(ScoreEvent scoreEvent)
    {
        switch (scoreEvent.Type)
        {
            case ScoreEventType.SuccessfulLanding:
                Statistics.SuccessfulLandings++;
                break;
            case ScoreEventType.SuccessfulHandoff:
                Statistics.SuccessfulHandoffs++;
                break;
            case ScoreEventType.SeparationViolation:
                Statistics.SeparationViolations++;
                break;
            case ScoreEventType.CommandIssued:
                Statistics.TotalCommands++;
                break;
            case ScoreEventType.AircraftSpawned:
                Statistics.TotalAircraft++;
                break;
        }

        // Track violations by severity
        if (scoreEvent.Severity != SeverityLevel.None)
        {
            Statistics.TotalViolations++;

            switch (scoreEvent.Severity)
            {
                case SeverityLevel.Minor:
                    Statistics.MinorViolations++;
                    break;
                case SeverityLevel.Moderate:
                    Statistics.ModerateViolations++;
                    break;
                case SeverityLevel.Major:
                    Statistics.MajorViolations++;
                    break;
                case SeverityLevel.Critical:
                    Statistics.CriticalViolations++;
                    break;
            }
        }
    }
}

/// <summary>
/// Statistical information about a session
/// </summary>
public class SessionStatistics
{
    public int TotalAircraft { get; set; }
    public int SuccessfulLandings { get; set; }
    public int SuccessfulHandoffs { get; set; }
    public int TotalCommands { get; set; }
    public int TotalViolations { get; set; }
    public int SeparationViolations { get; set; }
    public int MinorViolations { get; set; }
    public int ModerateViolations { get; set; }
    public int MajorViolations { get; set; }
    public int CriticalViolations { get; set; }

    /// <summary>
    /// Calculates efficiency (landings per command)
    /// </summary>
    public float GetEfficiency()
    {
        return TotalCommands > 0 ? (float)SuccessfulLandings / TotalCommands : 0;
    }

    /// <summary>
    /// Calculates safety rating (0-100)
    /// </summary>
    public float GetSafetyRating()
    {
        if (TotalAircraft == 0) return 100;

        // Deduct points based on violation severity
        var deductions = (MinorViolations * 1) +
                        (ModerateViolations * 5) +
                        (MajorViolations * 15) +
                        (CriticalViolations * 30);

        var rating = 100 - (deductions * 100.0f / TotalAircraft);
        return Math.Max(0, Math.Min(100, rating));
    }

    /// <summary>
    /// Calculates landing success rate (%)
    /// </summary>
    public float GetLandingSuccessRate()
    {
        return TotalAircraft > 0 ? (float)SuccessfulLandings / TotalAircraft * 100 : 0;
    }
}
