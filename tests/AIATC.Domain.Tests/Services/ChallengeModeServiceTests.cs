using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AIATC.Domain.Services;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Scenarios;

namespace AIATC.Domain.Tests.Services
{
    public class ChallengeModeServiceTests
    {
        private readonly Mock<AIAgentService> _mockAiAgent;
        private readonly Mock<ILogger<ChallengeModeService>> _mockLogger;
        private readonly ChallengeModeService _service;

        public ChallengeModeServiceTests()
        {
            _mockAiAgent = new Mock<AIAgentService>();
            _mockLogger = new Mock<ILogger<ChallengeModeService>>();
            _service = new ChallengeModeService(_mockAiAgent.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InitializeChallenge_WithValidScenario_ShouldSetReadyState()
        {
            // Act
            var result = await _service.InitializeChallengeAsync("demo-scenario", Difficulty.Intermediate);

            // Assert
            Assert.True(result);
            Assert.Equal(ChallengeState.Ready, _service.State);
            Assert.False(_service.IsActive);
            Assert.NotNull(_service.ChallengeId);
            Assert.Equal("demo-scenario", _service.ScenarioId);
        }

        [Fact]
        public void StartChallenge_FromReadyState_ShouldTransitionToRunning()
        {
            // Arrange
            _service.State = ChallengeState.Ready;

            // Act
            _service.StartChallenge();

            // Assert
            Assert.Equal(ChallengeState.Running, _service.State);
            Assert.True(_service.IsActive);
            Assert.NotNull(_service.StartTime);
            Assert.Equal(0, _service.SimulationTimeSeconds);
        }

        [Fact]
        public void StartChallenge_FromWrongState_ShouldNotTransition()
        {
            // Arrange
            _service.State = ChallengeState.Running;

            // Act
            _service.StartChallenge();

            // Assert
            Assert.Equal(ChallengeState.Running, _service.State);
        }

        [Fact]
        public void PauseChallenge_WhenRunning_ShouldTransitionToPaused()
        {
            // Arrange
            _service.State = ChallengeState.Running;

            // Act
            _service.PauseChallenge();

            // Assert
            Assert.Equal(ChallengeState.Paused, _service.State);
        }

        [Fact]
        public void ResumeChallenge_WhenPaused_ShouldTransitionToRunning()
        {
            // Arrange
            _service.State = ChallengeState.Paused;

            // Act
            _service.ResumeChallenge();

            // Assert
            Assert.Equal(ChallengeState.Running, _service.State);
        }

        [Fact]
        public void UpdateChallenge_WhenRunning_ShouldAdvanceTime()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.TimeMultiplier = 1.0f;

            // Act
            _service.UpdateChallenge(0.1f); // 100ms

            // Assert
            Assert.Equal(0.1f, _service.RealTimeSeconds);
            Assert.Equal(0.1f, _service.SimulationTimeSeconds);
        }

        [Fact]
        public void UpdateChallenge_WithTimeMultiplier_ShouldScaleSimulationTime()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.TimeMultiplier = 2.0f;

            // Act
            _service.UpdateChallenge(0.1f); // 100ms real time

            // Assert
            Assert.Equal(0.1f, _service.RealTimeSeconds);
            Assert.Equal(0.2f, _service.SimulationTimeSeconds); // Scaled by 2x
        }

        [Fact]
        public void UpdateChallenge_WhenPaused_ShouldNotAdvanceTime()
        {
            // Arrange
            _service.State = ChallengeState.Paused;
            float initialSimTime = _service.SimulationTimeSeconds;

            // Act
            _service.UpdateChallenge(0.1f);

            // Assert
            Assert.Equal(initialSimTime, _service.SimulationTimeSeconds);
        }

        [Fact]
        public void SubmitUserCommand_ShouldAddToCommandHistory()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.SimulationTimeSeconds = 10.0f;
            var aircraft = CreateTestAircraft();

            // Act
            _service.SubmitUserCommand("turn right heading 270", aircraft);

            // Assert
            Assert.Single(_service.UserCommandHistory);
            Assert.Equal("turn right heading 270", _service.UserCommandHistory[0].CommandText);
            Assert.Equal("TEST001", _service.UserCommandHistory[0].TargetCallsign);
            Assert.Equal(10.0f, _service.UserCommandHistory[0].Timestamp);
            Assert.False(_service.UserCommandHistory[0].IssuedByAI);
        }

        [Fact]
        public void SubmitUserCommand_WhenNotRunning_ShouldNotAddCommand()
        {
            // Arrange
            _service.State = ChallengeState.Paused;
            var aircraft = CreateTestAircraft();

            // Act
            _service.SubmitUserCommand("turn right heading 270", aircraft);

            // Assert
            Assert.Empty(_service.UserCommandHistory);
        }

        [Fact]
        public void GetCurrentComparison_ShouldReturnValidData()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.SimulationTimeSeconds = 25.5f;

            // Act
            var comparison = _service.GetCurrentComparison();

            // Assert
            Assert.NotNull(comparison);
            Assert.Equal(25.5f, comparison.SimulationTimeSeconds);
            Assert.NotNull(comparison.UserScore);
            Assert.NotNull(comparison.AiScore);
            Assert.True(comparison.UserAircraftCount >= 0);
            Assert.True(comparison.AiAircraftCount >= 0);
        }

        [Fact]
        public void EndChallenge_WhenRunning_ShouldTransitionToCompleted()
        {
            // Arrange
            _service.State = ChallengeState.Running;

            // Act
            _service.EndChallenge();

            // Assert
            Assert.Equal(ChallengeState.Completed, _service.State);
            Assert.False(_service.IsActive);
            Assert.NotNull(_service.EndTime);
        }

        [Fact]
        public void DetermineWinner_ShouldReturnValidResult()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.EndChallenge();

            // Act
            var result = _service.DetermineWinner();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Winner);
            Assert.True(result.Winner == "Human" || result.Winner == "AI" || result.Winner == "Tie");
            Assert.True(result.UserScore >= 0);
            Assert.True(result.AiScore >= 0);
            Assert.NotEmpty(result.ChallengeId);
        }

        [Fact]
        public void GetCommandHistory_ForUser_ShouldReturnUserCommands()
        {
            // Arrange
            var aircraft = CreateTestAircraft();
            _service.State = ChallengeState.Running;
            _service.SubmitUserCommand("descend to 3000", aircraft);
            _service.SubmitUserCommand("reduce speed to 180", aircraft);

            // Act
            var userHistory = _service.GetCommandHistory(forUser: true);

            // Assert
            Assert.Equal(2, userHistory.Count);
            Assert.All(userHistory, cmd => Assert.False(cmd.IssuedByAI));
        }

        [Fact]
        public void GetSimulationStates_ShouldReturnBothSimulations()
        {
            // Act
            var (userSim, aiSim) = _service.GetSimulationStates();

            // Assert
            Assert.NotNull(userSim);
            Assert.NotNull(aiSim);
            Assert.NotSame(userSim, aiSim); // Different instances
        }

        [Fact]
        public void StateChangeEvent_ShouldFireWhenStateChanges()
        {
            // Arrange
            _service.State = ChallengeState.Ready;
            var stateChangesFired = new List<ChallengeState>();
            _service.OnStateChanged += (s, e) => stateChangesFired.Add(e.NewState);

            // Act
            _service.StartChallenge();
            _service.PauseChallenge();

            // Assert
            Assert.Equal(2, stateChangesFired.Count);
            Assert.Equal(ChallengeState.Running, stateChangesFired[0]);
            Assert.Equal(ChallengeState.Paused, stateChangesFired[1]);
        }

        [Fact]
        public void ShowAIPredictions_ShouldToggle()
        {
            // Arrange
            bool initial = _service.ShowAIPredictions;

            // Act
            _service.ShowAIPredictions = !initial;

            // Assert
            Assert.NotEqual(initial, _service.ShowAIPredictions);
        }

        [Fact]
        public void TimeMultiplier_ShouldAffectSimulationSpeed()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.TimeMultiplier = 4.0f;

            // Act
            _service.UpdateChallenge(0.1f);
            float simTimeAfterUpdate = _service.SimulationTimeSeconds;

            // Assert
            Assert.Equal(0.4f, simTimeAfterUpdate); // 0.1 * 4.0
        }

        [Fact]
        public void MultipleUpdates_ShouldAccumulateTime()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            _service.TimeMultiplier = 1.0f;

            // Act
            _service.UpdateChallenge(0.1f);
            _service.UpdateChallenge(0.1f);
            _service.UpdateChallenge(0.1f);

            // Assert
            Assert.Equal(0.3f, _service.SimulationTimeSeconds, 4);
            Assert.Equal(0.3f, _service.RealTimeSeconds, 4);
        }

        [Fact]
        public void GetCurrentComparison_CommandCounts_ShouldMatchHistory()
        {
            // Arrange
            _service.State = ChallengeState.Running;
            var aircraft = CreateTestAircraft();

            _service.SubmitUserCommand("turn right heading 270", aircraft);
            _service.SubmitUserCommand("climb to 5000", aircraft);

            // Act
            var comparison = _service.GetCurrentComparison();

            // Assert
            Assert.Equal(2, comparison.UserCommandCount);
        }

        [Theory]
        [InlineData(ChallengeState.NotStarted)]
        [InlineData(ChallengeState.Paused)]
        [InlineData(ChallengeState.Completed)]
        public void EndChallenge_FromVariousStates_ShouldEndIfApplicable(ChallengeState initialState)
        {
            // Arrange
            _service.State = initialState;

            // Act
            _service.EndChallenge();

            // Assert
            if (initialState == ChallengeState.Running || initialState == ChallengeState.Paused)
            {
                Assert.Equal(ChallengeState.Completed, _service.State);
            }
            else
            {
                Assert.NotEqual(ChallengeState.Completed, _service.State);
            }
        }

        // Helper methods

        private AircraftModel CreateTestAircraft()
        {
            return new AircraftModel
            {
                Callsign = "TEST001",
                AircraftType = "B738",
                AltitudeFt = 5000,
                SpeedKnots = 250,
                HeadingDegrees = 180,
                TargetAltitudeFt = 3000,
                TargetSpeedKnots = 200,
                TargetHeadingDegrees = 270
            };
        }
    }
}
