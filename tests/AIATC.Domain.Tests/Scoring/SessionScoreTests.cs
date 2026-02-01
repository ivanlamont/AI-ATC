using Xunit;
using AIATC.Domain.Models.Scoring;
using System;
using System.Linq;

namespace AIATC.Domain.Tests.Scoring;

public class SessionScoreTests
{
    [Fact]
    public void SessionScore_InitializesWithDefaults()
    {
        var session = new SessionScore { SessionId = "test-session" };

        Assert.Equal("test-session", session.SessionId);
        Assert.Equal(0, session.TotalScore);
        Assert.Equal(0, session.BaseScore);
        Assert.Equal(1.0f, session.TimeMultiplier);
        Assert.Empty(session.Events);
    }

    [Fact]
    public void AddEvent_IncreasesScore()
    {
        var session = new SessionScore();
        var scoreEvent = new ScoreEvent
        {
            Type = ScoreEventType.SuccessfulLanding,
            Points = 100
        };

        session.AddEvent(scoreEvent);

        Assert.Equal(100, session.BaseScore);
        Assert.Equal(100, session.TotalScore);
        Assert.Single(session.Events);
    }

    [Fact]
    public void AddEvent_AppliesTimeMultiplier()
    {
        var session = new SessionScore { TimeMultiplier = 2.0f };
        var scoreEvent = new ScoreEvent
        {
            Type = ScoreEventType.SuccessfulLanding,
            Points = 100
        };

        session.AddEvent(scoreEvent);

        Assert.Equal(100, session.BaseScore);
        Assert.Equal(200, session.TotalScore); // Base * multiplier
    }

    [Fact]
    public void AddEvent_HandlesNegativePoints()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent { Points = 100 });
        session.AddEvent(new ScoreEvent { Points = -25 });

        Assert.Equal(75, session.BaseScore);
        Assert.Equal(75, session.TotalScore);
    }

    [Fact]
    public void GetEventsByType_FiltersCorrectly()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SuccessfulLanding, Points = 100 });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SuccessfulLanding, Points = 100 });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SeparationViolation, Points = -50 });

        var landings = session.GetEventsByType(ScoreEventType.SuccessfulLanding);

        Assert.Equal(2, landings.Count);
    }

    [Fact]
    public void GetViolations_ReturnsOnlyViolations()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SuccessfulLanding,
            Severity = SeverityLevel.None
        });

        session.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SeparationViolation,
            Severity = SeverityLevel.Major
        });

        var violations = session.GetViolations();

        Assert.Single(violations);
        Assert.Equal(ScoreEventType.SeparationViolation, violations[0].Type);
    }

    [Fact]
    public void Statistics_TracksSuccessfulLandings()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SuccessfulLanding });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SuccessfulLanding });

        Assert.Equal(2, session.Statistics.SuccessfulLandings);
    }

    [Fact]
    public void Statistics_TracksViolationsBySeverity()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SeparationViolation,
            Severity = SeverityLevel.Minor
        });

        session.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SeparationViolation,
            Severity = SeverityLevel.Critical
        });

        Assert.Equal(2, session.Statistics.TotalViolations);
        Assert.Equal(1, session.Statistics.MinorViolations);
        Assert.Equal(1, session.Statistics.CriticalViolations);
    }

    [Fact]
    public void GetSafetyRating_ReturnsCorrectValue()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent { Type = ScoreEventType.AircraftSpawned });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.AircraftSpawned });

        // No violations = 100% safety
        Assert.Equal(100, session.Statistics.GetSafetyRating());
    }

    [Fact]
    public void GetEfficiency_CalculatesCorrectly()
    {
        var session = new SessionScore();

        session.AddEvent(new ScoreEvent { Type = ScoreEventType.CommandIssued });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.CommandIssued });
        session.AddEvent(new ScoreEvent { Type = ScoreEventType.SuccessfulLanding });

        // 1 landing / 2 commands = 0.5 efficiency
        Assert.Equal(0.5f, session.Statistics.GetEfficiency());
    }
}
