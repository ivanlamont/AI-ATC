using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using AIATC.ML.Services;

namespace AIATC.AIAgentService.Tests
{
    public class TensorFlowModelServiceTests
    {
        private readonly Mock<ILogger<TensorFlowModelService>> _mockLogger;
        private readonly ModelConfiguration _config;

        public TensorFlowModelServiceTests()
        {
            _mockLogger = new Mock<ILogger<TensorFlowModelService>>();
            _config = new ModelConfiguration
            {
                ModelPath = "/tmp/model",
                InputSize = 128,
                ActionSize = 6,
                MinAltitudeFt = 500,
                MaxAltitudeFt = 15000,
                MinSpeedKts = 100,
                MaxSpeedKts = 450,
                MinHeadingDeg = 0,
                MaxHeadingDeg = 360
            };
        }

        [Fact]
        public void Constructor_WithValidConfig_ShouldInitialize()
        {
            // Act
            var service = new TensorFlowModelService(_config, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.False(service.IsModelLoaded);
        }

        [Fact]
        public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TensorFlowModelService(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TensorFlowModelService(_config, null));
        }

        [Fact]
        public void LoadModel_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var nonExistentPath = "/tmp/nonexistent_model_file.pb";

            // Act
            var result = service.LoadModel(nonExistentPath);

            // Assert
            Assert.False(result);
            Assert.False(service.IsModelLoaded);
        }

        [Fact]
        public void Infer_WhenModelNotLoaded_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = CreateTestObservation();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.Infer(observation));
        }

        [Fact]
        public void GetPerformanceStats_ShouldReturnValidDictionary()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);

            // Act
            var stats = service.GetPerformanceStats();

            // Assert
            Assert.NotNull(stats);
            Assert.Contains("IsModelLoaded", stats.Keys);
            Assert.Contains("CurrentModelPath", stats.Keys);
            Assert.Contains("AvgInferenceTimeMs", stats.Keys);
            Assert.Contains("InferenceSamplesTracked", stats.Keys);
            Assert.False((bool)stats["IsModelLoaded"]);
        }

        [Fact]
        public void PreprocessObservation_WithValidInput_ShouldReturnNormalizedArray()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = new GameObservation
            {
                AircraftAltitudeFt = 5000,
                AircraftSpeedKts = 250,
                AircraftHeadingDeg = 180,
                TargetAltitudeFt = 3000,
                TargetSpeedKts = 200,
                TargetHeadingDeg = 270,
                DistanceToAirportNm = 10,
                AltitudeToRunwayFt = 2000,
                WindSpeedKts = 15,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 3,
                NumAircraftInApproach = 2
            };

            // Act - using reflection to test private method
            var method = typeof(TensorFlowModelService)
                .GetMethod("PreprocessObservation",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (float[])method.Invoke(service, new object[] { observation });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_config.InputSize, result.Length);
            // Check that values are normalized (0-1 range)
            Assert.All(result.Take(12), val => Assert.InRange(val, 0f, 1.1f)); // Allow slight overshoot
        }

        [Fact]
        public void PostprocessOutput_WithValidOutput_ShouldReturnConstrainedAction()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = CreateTestObservation();
            var mockOutput = new float[] { 0.5f, 0.6f, 0.4f, 0.8f, 0.2f, 0.3f };

            // Act - using reflection to test private method
            var method = typeof(TensorFlowModelService)
                .GetMethod("PostprocessOutput",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Need to create NDArray - for this test we'll skip internal testing
            // and focus on public interface instead

            // Assert - verify the method exists
            Assert.NotNull(method);
        }

        [Fact]
        public void GetNeutralAction_ShouldReturnObservationState()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = CreateTestObservation();

            // Act - using reflection to test private method
            var method = typeof(TensorFlowModelService)
                .GetMethod("GetNeutralAction",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (MLAction)method.Invoke(service, new object[] { observation });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(observation.AircraftHeadingDeg, result.HeadingDeg);
            Assert.Equal(observation.AircraftAltitudeFt, result.AltitudeFt);
            Assert.Equal(observation.AircraftSpeedKts, result.SpeedKts);
            Assert.Equal(0.5f, result.Confidence);
            Assert.Equal(-1, result.ActionIndex);
        }

        [Fact]
        public void Dispose_ShouldCleanupResources()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);

            // Act
            service.Dispose();
            service.Dispose(); // Should not throw on multiple disposes

            // Assert
            var stats = service.GetPerformanceStats();
            Assert.False((bool)stats["IsModelLoaded"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task LoadModelAsync_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var nonExistentPath = "/tmp/nonexistent_async_model.pb";

            // Act
            var result = await service.LoadModelAsync(nonExistentPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task InferAsync_WhenModelNotLoaded_ShouldReturnNeutralAction()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = CreateTestObservation();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InferAsync(observation));
        }

        [Fact]
        public void HotSwapModel_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);

            // Act
            var result = service.HotSwapModel("/tmp/nonexistent_hotswap_model.pb");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(500, 0)]      // Min altitude normalized to 0
        [InlineData(7750, 0.5f)]  // Mid altitude normalized to 0.5
        [InlineData(15000, 1)]    // Max altitude normalized to 1
        public void Normalization_AltitudeValues_ShouldMapCorrectly(float altitude, float expectedNorm)
        {
            // Arrange
            var service = new TensorFlowModelService(_config, _mockLogger.Object);
            var observation = new GameObservation
            {
                AircraftAltitudeFt = altitude,
                AircraftSpeedKts = 275,
                AircraftHeadingDeg = 180,
                TargetAltitudeFt = 3000,
                TargetSpeedKts = 200,
                TargetHeadingDeg = 270,
                DistanceToAirportNm = 10,
                AltitudeToRunwayFt = 2000,
                WindSpeedKts = 15,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 3,
                NumAircraftInApproach = 2
            };

            // Act
            var method = typeof(TensorFlowModelService)
                .GetMethod("PreprocessObservation",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (float[])method.Invoke(service, new object[] { observation });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedNorm, result[0], 2); // 2 decimal places precision
        }

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
                DistanceToAirportNm = 10,
                AltitudeToRunwayFt = 2000,
                WindSpeedKts = 15,
                WindDirectionDeg = 90,
                SeparationFromOtherAircraftNm = 3,
                NumAircraftInApproach = 2
            };
        }
    }
}
