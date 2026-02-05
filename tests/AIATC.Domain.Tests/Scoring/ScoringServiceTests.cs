using Xunit;
using AIATC.Domain.Models.Scoring;
using AIATC.Domain.Models.Commands;

namespace AIATC.Domain.Tests.Scoring;

public class ScoringServiceTests
{
    [Fact]
    public void StartNewSession_InitializesSession()
    {
        var service = new ScoringService();

        service.StartNewSession("test-session", 2.0f);

        var session = service.GetCurrentSession();
        Assert.Equal("test-session", session.SessionId);
        Assert.Equal(2.0f, session.TimeMultiplier);
    }

    [Fact]
    public void RegisterAircraft_AddsToSession()
    {
        var service = new ScoringService();
        service.StartNewSession("test");

        service.RegisterAircraft("UAL123", 50.0f);

        var happiness = service.GetAircraftHappiness("UAL123");
        Assert.NotNull(happiness);
        Assert.Equal("UAL123", happiness.Callsign);
        Assert.Equal(50.0f, happiness.DirectDistance);
    }

    [Fact]
    public void RecordLanding_AddsPositiveScore()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordLanding("UAL123");

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore > 0);
        Assert.True(session.Statistics.SuccessfulLandings == 1);
    }

    [Fact]
    public void RecordHandoff_AddsScore()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordHandoff("UAL123", "Tower");

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore > 0);
        Assert.Equal(1, session.Statistics.SuccessfulHandoffs);
    }

    [Fact]
    public void RecordCommand_TracksCommandCount()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        var command = new HeadingCommand { TargetHeadingDegrees = 270 };
        service.RecordCommand("UAL123", command);

        var happiness = service.GetAircraftHappiness("UAL123");
        Assert.Equal(1, happiness!.CommandCount);
        Assert.Equal(1, service.GetCurrentSession().Statistics.TotalCommands);
    }

    [Fact]
    public void RecordCommand_PenalizesExcessiveCommands()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        var command = new HeadingCommand { TargetHeadingDegrees = 270 };

        // Issue 15 commands
        for (int i = 0; i < 15; i++)
        {
            service.RecordCommand("UAL123", command);
        }

        var session = service.GetCurrentSession();
        var happiness = service.GetAircraftHappiness("UAL123");

        // Should have penalties for excessive commands
        Assert.True(happiness!.Happiness < 100);
        Assert.True(session.TotalScore < 0); // Penalties accumulated
    }

    [Fact]
    public void RecordSeparationViolation_DeductsPoints()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);
        service.RegisterAircraft("DAL456", 50.0f);

        service.RecordSeparationViolation("UAL123", "DAL456", 2.0f);

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore < 0);
        Assert.Equal(1, session.Statistics.SeparationViolations);

        var h1 = service.GetAircraftHappiness("UAL123");
        var h2 = service.GetAircraftHappiness("DAL456");

        Assert.True(h1!.Happiness < 100);
        Assert.True(h2!.Happiness < 100);
    }

    [Fact]
    public void RecordSeparationViolation_SeverityBasedOnDistance()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);
        service.RegisterAircraft("DAL456", 50.0f);

        // Critical violation (< 1 NM)
        service.RecordSeparationViolation("UAL123", "DAL456", 0.5f);

        var violations = service.GetCurrentSession().GetViolations();
        Assert.Single(violations);
        Assert.Equal(SeverityLevel.Critical, violations[0].Severity);
    }

    [Fact]
    public void RecordAltitudeViolation_DeductsPoints()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordAltitudeViolation("UAL123", 5000, 5800);

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore < 0);

        var happiness = service.GetAircraftHappiness("UAL123");
        Assert.True(happiness!.Happiness < 100);
    }

    [Fact]
    public void RecordSpeedViolation_DeductsPoints()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordSpeedViolation("UAL123", 180, 220);

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore < 0);
    }

    [Fact]
    public void RecordEfficientRoute_AddsBonus()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordEfficientRoute("UAL123", 0.95f);

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore > 0);

        var happiness = service.GetAircraftHappiness("UAL123");
        Assert.True(happiness!.Happiness > 100 || happiness.Happiness == 100);
    }

    [Fact]
    public void RecordProcedureCompliance_AddsBonus()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.RecordProcedureCompliance("UAL123", "BDEGA2");

        var session = service.GetCurrentSession();
        Assert.True(session.TotalScore > 0);
    }

    [Fact]
    public void UpdateHoldingTime_PenalizesExtendedHolding()
    {
        var service = new ScoringService();
        service.StartNewSession("test");
        service.RegisterAircraft("UAL123", 50.0f);

        service.UpdateHoldingTime("UAL123", 600); // 10 minutes

        var happiness = service.GetAircraftHappiness("UAL123");
        Assert.True(happiness!.Happiness < 100);
    }

    [Fact]
    public void EndSession_SetsEndTime()
    {
        var service = new ScoringService();
        service.StartNewSession("test");

        service.EndSession();

        var session = service.GetCurrentSession();
        Assert.NotNull(session.EndTime);
    }

    [Fact]
    public void TimeMultiplier_AffectsFinalScore()
    {
        var service1 = new ScoringService();
        service1.StartNewSession("test1", 1.0f);
        service1.RegisterAircraft("UAL123", 50.0f);
        service1.RecordLanding("UAL123");

        var service2 = new ScoringService();
        service2.StartNewSession("test2", 2.0f);
        service2.RegisterAircraft("UAL123", 50.0f);
        service2.RecordLanding("UAL123");

        var score1 = service1.GetCurrentSession().TotalScore;
        var score2 = service2.GetCurrentSession().TotalScore;

        // 2x multiplier should give roughly 2x score
        Assert.True(score2 > score1);
        Assert.True(score2 >= score1 * 1.8f); // Allow some variance
    }
}
