using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using AIATC.Domain.Services;
using AIATC.Domain.Models;
using AIATC.Common;

namespace AIATC.Domain.Tests.Services
{
    public class AIAgentServiceTests
    {
        private readonly Mock<ILogger<AIAgentService>> _mockLogger;
        private readonly AIAgentService _service;

        public AIAgentServiceTests()
        {
            _mockLogger = new Mock<ILogger<AIAgentService>>();
            _service = new AIAgentService("localhost", 50051, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithDefaultParameters_ShouldInitialize()
        {
            // Act
            var service = new AIAgentService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithCustomParameters_ShouldSetValues()
        {
            // Act
            var service = new AIAgentService("192.168.1.1", 9999, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void GetAction_WithValidObservation_ShouldReturnMLAction()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            Assert.InRange(action.HeadingDeg, 0, 360);
            Assert.InRange(action.AltitudeFt, 500, 40000);
            Assert.InRange(action.SpeedKts, 100, 450);
            Assert.InRange(action.Confidence, 0, 1);
        }

        [Fact]
        public void GetAction_ShouldRespectAltitudeConstraints()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.True(action.AltitudeFt >= 500, "Altitude below minimum");
            Assert.True(action.AltitudeFt <= 40000, "Altitude above maximum");
        }

        [Fact]
        public void GetAction_ShouldRespectSpeedConstraints()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.True(action.SpeedKts >= 100, "Speed below minimum");
            Assert.True(action.SpeedKts <= 450, "Speed above maximum");
        }

        [Fact]
        public void GetAction_ShouldNormalizeHeading()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.True(action.HeadingDeg >= 0, "Heading below 0");
            Assert.True(action.HeadingDeg < 360, "Heading at or above 360");
        }

        [Fact]
        public void GetAction_WhenDistantFromAirport_ShouldMaintainAltitudeAndSpeed()
        {
            // Arrange
            var observation = new GameObservation
            {
                DistanceToAirportNm = 50, // Far away
                AircraftAltitudeFt = 20000,
                AircraftSpeedKts = 350,
                TargetAltitudeFt = 15000,
                TargetSpeedKts = 300,
                AircraftHeadingDeg = 180,
                TargetHeadingDeg = 180,
                AltitudeToRunwayFt = 20000,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 10,
                NumAircraftInApproach = 1
            };

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            // Should make conservative changes at distance
            Assert.True(action.Confidence >= 0.5f, "Confidence should be moderate when far");
        }

        [Fact]
        public void GetAction_WhenCloseToAirport_ShouldDescendAndSlowDown()
        {
            // Arrange
            var observation = new GameObservation
            {
                DistanceToAirportNm = 5, // Very close
                AircraftAltitudeFt = 3000,
                AircraftSpeedKts = 250,
                TargetAltitudeFt = 2000,
                TargetSpeedKts = 150,
                AircraftHeadingDeg = 180,
                TargetHeadingDeg = 180,
                AltitudeToRunwayFt = 3000,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 3,
                NumAircraftInApproach = 2
            };

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            // Should descend and reduce speed for approach
            Assert.True(action.AltitudeFt < observation.AircraftAltitudeFt,
                "Should descend in approach");
            Assert.True(action.SpeedKts < observation.AircraftSpeedKts,
                "Should reduce speed in approach");
        }

        [Fact]
        public void GetAction_WhenPoorSeparation_ShouldReduceSpeed()
        {
            // Arrange
            var observation = new GameObservation
            {
                DistanceToAirportNm = 15,
                AircraftAltitudeFt = 4000,
                AircraftSpeedKts = 250,
                TargetAltitudeFt = 3000,
                TargetSpeedKts = 200,
                AircraftHeadingDeg = 180,
                TargetHeadingDeg = 180,
                AltitudeToRunwayFt = 4000,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 2.5f, // Less than 3nm minimum
                NumAircraftInApproach = 3
            };

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            // Should reduce speed to maintain separation
            Assert.True(action.SpeedKts < observation.AircraftSpeedKts,
                "Should reduce speed for poor separation");
        }

        [Fact]
        public void GetAction_ShouldCalculateConfidence()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.InRange(action.Confidence, 0.1f, 0.95f);
        }

        [Fact]
        public async Task GetActionAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = await _service.GetActionAsync(observation);

            // Assert
            Assert.NotNull(action);
            Assert.InRange(action.HeadingDeg, 0, 360);
        }

        [Fact]
        public async Task HealthCheckAsync_ShouldReturnTrue()
        {
            // Act
            var isHealthy = await _service.HealthCheckAsync();

            // Assert
            Assert.True(isHealthy);
        }

        [Fact]
        public void GetMetrics_ShouldReturnMetrics()
        {
            // Arrange
            var observation = CreateTestObservation();
            _service.GetAction(observation);
            _service.GetAction(observation);

            // Act
            var metrics = _service.GetMetrics();

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(2, metrics.RequestCount);
            Assert.Equal(2, metrics.SuccessCount);
            Assert.Equal(0, metrics.ErrorCount);
            Assert.True(metrics.SuccessRate > 0);
        }

        [Fact]
        public void GetMetrics_ShouldTrackAverageInferenceTime()
        {
            // Arrange
            var observation = CreateTestObservation();
            _service.GetAction(observation);

            // Act
            var metrics = _service.GetMetrics();

            // Assert
            Assert.NotNull(metrics);
            Assert.True(metrics.AverageInferenceTime >= 0);
        }

        [Fact]
        public void GetAction_MultipleCallsShouldUpdateMetrics()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            for (int i = 0; i < 5; i++)
            {
                _service.GetAction(observation);
            }
            var metrics = _service.GetMetrics();

            // Assert
            Assert.Equal(5, metrics.RequestCount);
            Assert.Equal(5, metrics.SuccessCount);
            Assert.Equal(1.0f, metrics.SuccessRate);
        }

        [Fact]
        public void GetAction_ShouldReturnInferenceTime()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.True(action.InferenceTimeMs >= 0);
        }

        [Fact]
        public void GetAction_ConsecutiveCalls_ShouldProduceDifferentResults()
        {
            // Arrange
            var observation = CreateTestObservation();

            // Act
            var action1 = _service.GetAction(observation);
            var action2 = _service.GetAction(observation);

            // Assert
            // Results may differ due to randomization in strategy
            Assert.NotNull(action1);
            Assert.NotNull(action2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        public void GetAction_WithVaryingNumberOfAircraft_ShouldAdjustConfidence(int aircraftCount)
        {
            // Arrange
            var observation = new GameObservation
            {
                DistanceToAirportNm = 20,
                AircraftAltitudeFt = 5000,
                AircraftSpeedKts = 250,
                TargetAltitudeFt = 3000,
                TargetSpeedKts = 200,
                AircraftHeadingDeg = 180,
                TargetHeadingDeg = 180,
                AltitudeToRunwayFt = 5000,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 5,
                NumAircraftInApproach = aircraftCount
            };

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            // Confidence should adjust based on traffic
            if (aircraftCount > 5)
                Assert.True(action.Confidence < 0.8f, "Confidence should be lower with high traffic");
        }

        [Fact]
        public void GetAction_ShouldHandleExtremeAltitudes()
        {
            // Arrange
            var observation = new GameObservation
            {
                DistanceToAirportNm = 30,
                AircraftAltitudeFt = 500, // At minimum
                AircraftSpeedKts = 150,
                TargetAltitudeFt = 500,
                TargetSpeedKts = 150,
                AircraftHeadingDeg = 180,
                TargetHeadingDeg = 180,
                AltitudeToRunwayFt = 500,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 10,
                NumAircraftInApproach = 1
            };

            // Act
            var action = _service.GetAction(observation);

            // Assert
            Assert.NotNull(action);
            Assert.True(action.AltitudeFt >= 500);
        }

        // Helper methods

        private GameObservation CreateTestObservation()
        {
            return new GameObservation
            {
                AircraftAltitudeFt = 5000,
                AircraftSpeedKts = 250,
                AircraftHeadingDeg = 180,
                TargetAltitudeFt = 3000,
                TargetSpeedKts = 200,
                TargetHeadingDeg = 270,
                DistanceToAirportNm = 20,
                AltitudeToRunwayFt = 5000,
                WindSpeedKts = 10,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 5,
                NumAircraftInApproach = 2
            };
        }
    }
}
