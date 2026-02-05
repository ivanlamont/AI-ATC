using System;
using System.Threading.Tasks;
using AIATC.Domain.Models;
using Microsoft.Extensions.Logging;
using AIATC.Common;

namespace AIATC.Domain.Services
{
    /// <summary>
    /// AI Agent Service - Interface to ML model for getting ATC decisions
    ///
    /// Handles communication with either:
    /// - Local TensorFlow.NET model (in-process)
    /// - Remote gRPC service (distributed inference)
    /// </summary>
    public interface IAIAgentService
    {
        /// <summary>
        /// Get AI action for a given observation
        /// </summary>
        MLAction GetAction(GameObservation observation);

        /// <summary>
        /// Asynchronously get AI action
        /// </summary>
        Task<MLAction> GetActionAsync(GameObservation observation);

        /// <summary>
        /// Check if AI agent is ready
        /// </summary>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// Get performance metrics
        /// </summary>
        AIAgentMetrics GetMetrics();
    }

    /// <summary>
    /// Default AI Agent Service implementation using gRPC
    /// </summary>
    public class AIAgentService : IAIAgentService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger<AIAgentService> _logger;
        private AIAgentMetrics _metrics;

        public AIAgentService(
            string host = "localhost",
            int port = 50051,
            ILogger<AIAgentService> logger = null)
        {
            _host = host ?? "localhost";
            _port = port;
            _logger = logger;
            _metrics = new AIAgentMetrics();
        }

        /// <summary>
        /// Get AI decision for current game state
        /// </summary>
        public MLAction GetAction(GameObservation observation)
        {
            try
            {
                var task = GetActionAsync(observation);
                task.Wait(TimeSpan.FromSeconds(5)); // 5 second timeout

                if (task.IsCompleted)
                    return task.Result;

                _logger.LogWarning("AI action request timed out, returning neutral action");
                return GetNeutralAction(observation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting AI action: {ex.Message}");
                return GetNeutralAction(observation);
            }
        }

        /// <summary>
        /// Asynchronously get AI decision
        /// </summary>
        public async Task<MLAction> GetActionAsync(GameObservation observation)
        {
            try
            {
                _metrics.RequestCount++;

                // In production, this would call remote gRPC service
                // For now, return deterministic AI response based on observation

                var action = ComputeAiDecision(observation);

                _metrics.SuccessCount++;
                _metrics.AverageInferenceTime = (_metrics.AverageInferenceTime * (_metrics.SuccessCount - 1) + action.InferenceTimeMs) / _metrics.SuccessCount;

                return action;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AI inference: {ex.Message}");
                _metrics.ErrorCount++;
                return GetNeutralAction(observation);
            }
        }

        /// <summary>
        /// Check if AI service is healthy
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                // Would call remote health check endpoint
                // For now, assume healthy
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        public AIAgentMetrics GetMetrics()
        {
            return _metrics;
        }

        // ============ Private Helper Methods ============

        /// <summary>
        /// Compute AI decision based on observation
        ///
        /// This is a placeholder implementation that uses heuristics.
        /// In production, this would call the TensorFlow.NET model or gRPC service.
        /// </summary>
        private MLAction ComputeAiDecision(GameObservation observation)
        {
            var random = new Random();
            var startTime = DateTime.UtcNow;

            // AI Strategy: Approach descent with speed/altitude management

            float targetHeading = observation.TargetHeadingDeg;
            float targetAltitude = observation.TargetAltitudeFt;
            float targetSpeed = observation.TargetSpeedKts;

            // Adjust based on distance to airport
            if (observation.DistanceToAirportNm > 30)
            {
                // Distant: descend at moderate rate, maintain speed
                targetAltitude = Math.Min(observation.AircraftAltitudeFt - 500, observation.TargetAltitudeFt);
                targetSpeed = Math.Max(observation.AircraftSpeedKts - 20, 200); // Descend to approach speed
            }
            else if (observation.DistanceToAirportNm > 10)
            {
                // Medium distance: descend more aggressively
                targetAltitude = Math.Min(observation.AircraftAltitudeFt - 800, observation.TargetAltitudeFt);
                targetSpeed = Math.Max(observation.AircraftSpeedKts - 40, 180);
            }
            else
            {
                // Close approach: prepare for landing
                targetAltitude = Math.Min(observation.AircraftAltitudeFt - 300, 2000);
                targetSpeed = 150;
            }

            // Manage separation from other aircraft
            if (observation.SeparationFromOtherAircraftNm < 3.5f)
            {
                targetSpeed = Math.Min(observation.AircraftSpeedKts - 20, targetSpeed);
            }

            // Add small random variations for realistic behavior
            targetHeading += (float)(random.NextDouble() - 0.5) * 10;
            targetSpeed += (float)(random.NextDouble() - 0.5) * 5;

            // Ensure values stay in valid ranges
            targetHeading = NormalizeHeading(targetHeading);
            targetAltitude = Math.Clamp(targetAltitude, 500, 40000);
            targetSpeed = Math.Clamp(targetSpeed, 100, 450);

            // Calculate confidence based on certainty of decision
            float confidence = CalculateConfidence(observation, targetAltitude, targetSpeed);

            var inferenceTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new MLAction
            {
                HeadingDeg = targetHeading,
                AltitudeFt = targetAltitude,
                SpeedKts = targetSpeed,
                Confidence = confidence,
                ActionIndex = 0,
                InferenceTimeMs = (long)inferenceTime
            };
        }

        /// <summary>
        /// Get neutral/hold action (maintain current state)
        /// </summary>
        private MLAction GetNeutralAction(GameObservation observation)
        {
            return new MLAction
            {
                HeadingDeg = observation.AircraftHeadingDeg,
                AltitudeFt = observation.AircraftAltitudeFt,
                SpeedKts = observation.AircraftSpeedKts,
                Confidence = 0.5f,
                ActionIndex = -1,
                InferenceTimeMs = 0
            };
        }

        private float NormalizeHeading(float heading)
        {
            while (heading < 0) heading += 360;
            while (heading >= 360) heading -= 360;
            return heading;
        }

        private float CalculateConfidence(GameObservation observation, float targetAlt, float targetSpeed)
        {
            // Confidence is high when:
            // - Good separation from other aircraft
            // - Altitude change is reasonable
            // - Not in critical approach phase

            float separation = observation.SeparationFromOtherAircraftNm;
            float altitudeChange = Math.Abs(targetAlt - observation.AircraftAltitudeFt);
            float speedChange = Math.Abs(targetSpeed - observation.AircraftSpeedKts);
            float distance = observation.DistanceToAirportNm;

            float confidence = 0.7f; // Base confidence

            // Reduce confidence if many aircraft in approach
            if (observation.NumAircraftInApproach > 5)
                confidence -= 0.1f;

            // Reduce if poor separation
            if (separation < 4)
                confidence -= 0.15f;

            // Reduce if large altitude change needed
            if (altitudeChange > 2000)
                confidence -= 0.05f;

            // Increase if in good situation
            if (distance > 20 && separation > 5 && observation.NumAircraftInApproach <= 2)
                confidence += 0.1f;

            return Math.Clamp(confidence, 0.1f, 0.95f);
        }
    }

    /// <summary>
    /// Performance metrics for AI agent
    /// </summary>
    public class AIAgentMetrics
    {
        public int RequestCount { get; set; } = 0;
        public int SuccessCount { get; set; } = 0;
        public int ErrorCount { get; set; } = 0;
        public double AverageInferenceTime { get; set; } = 0; // milliseconds
        public DateTime? LastRequestTime { get; set; }

        public float SuccessRate => RequestCount > 0 ? (float)SuccessCount / RequestCount : 0;
    }
}
