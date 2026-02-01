using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Models.Scenarios;

/// <summary>
/// Represents a complete scenario
/// </summary>
public class Scenario
{
    /// <summary>
    /// Scenario metadata
    /// </summary>
    public ScenarioMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Scenario configuration
    /// </summary>
    public ScenarioConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Scenario objectives
    /// </summary>
    public List<ScenarioObjective> Objectives { get; set; } = new();

    /// <summary>
    /// Current state of the scenario
    /// </summary>
    public ScenarioState State { get; set; } = ScenarioState.NotStarted;

    /// <summary>
    /// Start time (UTC)
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// End time (UTC)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Elapsed time in seconds
    /// </summary>
    public float ElapsedTimeSeconds { get; set; }

    /// <summary>
    /// Current score
    /// </summary>
    public int CurrentScore { get; set; }

    /// <summary>
    /// Number of aircraft spawned
    /// </summary>
    public int AircraftSpawned { get; set; }

    /// <summary>
    /// Number of aircraft landed
    /// </summary>
    public int AircraftLanded { get; set; }

    /// <summary>
    /// Number of separation violations
    /// </summary>
    public int SeparationViolations { get; set; }

    /// <summary>
    /// Scenario result (null if not complete)
    /// </summary>
    public ScenarioResult? Result { get; set; }

    /// <summary>
    /// Checks if all required objectives are complete
    /// </summary>
    public bool AreRequiredObjectivesComplete()
    {
        return Objectives
            .Where(o => o.IsRequired)
            .All(o => o.IsCompleted);
    }

    /// <summary>
    /// Gets overall completion percentage (0-100)
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (Objectives.Count == 0)
            return 0f;

        var totalPercentage = Objectives
            .Where(o => o.IsRequired)
            .Sum(o => o.GetCompletionPercentage());

        var requiredCount = Objectives.Count(o => o.IsRequired);
        return requiredCount > 0 ? totalPercentage / requiredCount : 0f;
    }

    /// <summary>
    /// Starts the scenario
    /// </summary>
    public void Start()
    {
        if (State != ScenarioState.NotStarted)
            throw new InvalidOperationException("Scenario already started");

        State = ScenarioState.Running;
        StartTime = DateTime.UtcNow;
        ElapsedTimeSeconds = 0f;
        CurrentScore = 0;
        AircraftSpawned = 0;
        AircraftLanded = 0;
        SeparationViolations = 0;
    }

    /// <summary>
    /// Updates the scenario state
    /// </summary>
    public void Update(float deltaTimeSeconds)
    {
        if (State != ScenarioState.Running)
            return;

        ElapsedTimeSeconds += deltaTimeSeconds;

        // Check time limit objective
        var timeLimitObjective = Objectives
            .FirstOrDefault(o => o.Type == ObjectiveType.TimeLimit);

        if (timeLimitObjective != null)
        {
            timeLimitObjective.UpdateProgress(ElapsedTimeSeconds);

            if (timeLimitObjective.IsCompleted && !AreRequiredObjectivesComplete())
            {
                // Time limit exceeded without completing objectives
                State = ScenarioState.Failed;
                EndTime = DateTime.UtcNow;
                Result = ScenarioResult.CreateFailed("Time limit exceeded");
            }
        }
    }

    /// <summary>
    /// Pauses the scenario
    /// </summary>
    public void Pause()
    {
        if (State == ScenarioState.Running)
        {
            State = ScenarioState.Paused;
        }
    }

    /// <summary>
    /// Resumes the scenario
    /// </summary>
    public void Resume()
    {
        if (State == ScenarioState.Paused)
        {
            State = ScenarioState.Running;
        }
    }

    /// <summary>
    /// Completes the scenario successfully
    /// </summary>
    public void Complete()
    {
        if (State != ScenarioState.Running && State != ScenarioState.Paused)
            return;

        State = ScenarioState.Completed;
        EndTime = DateTime.UtcNow;

        Result = ScenarioResult.CreateSuccess(
            CurrentScore,
            ElapsedTimeSeconds,
            AircraftLanded,
            SeparationViolations);
    }

    /// <summary>
    /// Fails the scenario
    /// </summary>
    public void Fail(string reason)
    {
        if (State != ScenarioState.Running && State != ScenarioState.Paused)
            return;

        State = ScenarioState.Failed;
        EndTime = DateTime.UtcNow;
        Result = ScenarioResult.CreateFailed(reason);
    }

    /// <summary>
    /// Gets the duration of the scenario
    /// </summary>
    public TimeSpan? GetDuration()
    {
        if (StartTime == null)
            return null;

        var end = EndTime ?? DateTime.UtcNow;
        return end - StartTime.Value;
    }
}

/// <summary>
/// Scenario state
/// </summary>
public enum ScenarioState
{
    /// <summary>
    /// Scenario not yet started
    /// </summary>
    NotStarted,

    /// <summary>
    /// Scenario is running
    /// </summary>
    Running,

    /// <summary>
    /// Scenario is paused
    /// </summary>
    Paused,

    /// <summary>
    /// Scenario completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Scenario failed
    /// </summary>
    Failed
}

/// <summary>
/// Result of a completed scenario
/// </summary>
public class ScenarioResult
{
    /// <summary>
    /// Whether the scenario was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Final score
    /// </summary>
    public int FinalScore { get; set; }

    /// <summary>
    /// Completion time in seconds
    /// </summary>
    public float CompletionTimeSeconds { get; set; }

    /// <summary>
    /// Number of aircraft landed
    /// </summary>
    public int AircraftLanded { get; set; }

    /// <summary>
    /// Number of separation violations
    /// </summary>
    public int Violations { get; set; }

    /// <summary>
    /// Star rating (1-5)
    /// </summary>
    public int StarRating { get; set; }

    /// <summary>
    /// Failure reason (if failed)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Performance grade (A-F)
    /// </summary>
    public string Grade { get; set; } = "F";

    /// <summary>
    /// Summary comments
    /// </summary>
    public List<string> Comments { get; set; } = new();

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static ScenarioResult CreateSuccess(int score, float time, int landed, int violations)
    {
        var result = new ScenarioResult
        {
            Success = true,
            FinalScore = score,
            CompletionTimeSeconds = time,
            AircraftLanded = landed,
            Violations = violations
        };

        // Calculate star rating (1-5)
        result.StarRating = CalculateStarRating(score, violations);

        // Calculate grade
        result.Grade = CalculateGrade(score, violations);

        // Add comments
        result.Comments = GenerateComments(score, landed, violations, time);

        return result;
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static ScenarioResult CreateFailed(string reason)
    {
        return new ScenarioResult
        {
            Success = false,
            FailureReason = reason,
            StarRating = 0,
            Grade = "F",
            Comments = new List<string> { reason }
        };
    }

    private static int CalculateStarRating(int score, int violations)
    {
        if (violations >= 5) return 1;
        if (violations >= 3) return 2;
        if (violations >= 1) return 3;
        if (score < 500) return 3;
        if (score < 1000) return 4;
        return 5;
    }

    private static string CalculateGrade(int score, int violations)
    {
        if (violations >= 5) return "F";
        if (violations >= 3) return "D";
        if (violations >= 2) return "C";
        if (score < 500) return "C";
        if (score < 750) return "B";
        if (score < 1000) return "A";
        return "A+";
    }

    private static List<string> GenerateComments(int score, int landed, int violations, float time)
    {
        var comments = new List<string>();

        // Aircraft landed
        if (landed >= 10)
            comments.Add($"Excellent work landing {landed} aircraft!");
        else if (landed >= 5)
            comments.Add($"Good job landing {landed} aircraft.");
        else
            comments.Add($"Landed {landed} aircraft.");

        // Violations
        if (violations == 0)
            comments.Add("Perfect separation! No violations.");
        else if (violations == 1)
            comments.Add("One separation violation - watch your spacing.");
        else
            comments.Add($"{violations} separation violations - needs improvement.");

        // Score
        if (score >= 1000)
            comments.Add("Outstanding score!");
        else if (score >= 750)
            comments.Add("Great score!");
        else if (score >= 500)
            comments.Add("Good score.");

        // Time
        var minutes = (int)(time / 60);
        if (minutes > 0)
            comments.Add($"Completed in {minutes} minutes.");

        return comments;
    }
}
