/// <summary>
/// gRPC Service for AI Agent Inference
///
/// Provides a network service for remote inference requests.
/// Allows other services to get AI-generated ATC commands.
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using AIATC.ML.Services;
using AIATC.Common;


namespace AIATC.ML.Services
{

    /// <summary>
    /// AI Agent inference service via gRPC
    /// </summary>
    public class AIAgentGrpcService
    {
        private readonly TensorFlowModelService _modelService;
        private readonly ILogger<AIAgentGrpcService> _logger;
        private Server _server;
        private readonly int _port;

        public bool IsRunning { get; private set; }
        public int Port => _port;

        public AIAgentGrpcService(
            TensorFlowModelService modelService,
            ILogger<AIAgentGrpcService> logger,
            int port = 50051)
        {
            _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _port = port;
        }

        /// <summary>
        /// Start gRPC server
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _server = new Server
                {
                    Services = { GetServiceDefinition() },
                    Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
                };

                await _server.StartAsync();
                IsRunning = true;

                _logger.LogInformation($"AI Agent gRPC service started on port {_port}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting gRPC server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop gRPC server
        /// </summary>
        public async Task StopAsync()
        {
            if (_server == null) return;

            try
            {
                await _server.ShutdownAsync();
                IsRunning = false;
                _logger.LogInformation("AI Agent gRPC service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping gRPC server: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle inference request
        /// </summary>
        public ActionMessage Infer(ObservationMessage request)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Convert to game observation
                var observation = new GameObservation
                {
                    AircraftAltitudeFt = request.AircraftAltitudeFt,
                    AircraftSpeedKts = request.AircraftSpeedKts,
                    AircraftHeadingDeg = request.AircraftHeadingDeg,
                    TargetAltitudeFt = request.TargetAltitudeFt,
                    TargetSpeedKts = request.TargetSpeedKts,
                    TargetHeadingDeg = request.TargetHeadingDeg,
                    DistanceToAirportNm = request.DistanceToAirportNm,
                    AltitudeToRunwayFt = request.AltitudeToRunwayFt,
                    WindSpeedKts = request.WindSpeedKts,
                    WindDirectionDeg = request.WindDirectionDeg,
                    SeparationFromOtherAircraftNm = request.SeparationFromOtherAircraftNm,
                    NumAircraftInApproach = request.NumAircraftInApproach,
                };

                // Run inference
                var mlAction = _modelService.Infer(observation);

                stopwatch.Stop();

                // Generate ATC command from action
                var command = GenerateATCCommand(mlAction, observation);

                _logger.LogDebug($"Inference completed: {command} (confidence: {mlAction.Confidence:F2})");

                return new ActionMessage
                {
                    HeadingDeg = mlAction.HeadingDeg,
                    AltitudeFt = mlAction.AltitudeFt,
                    SpeedKts = mlAction.SpeedKts,
                    Confidence = mlAction.Confidence,
                    Command = command,
                    InferenceTimeMs = stopwatch.ElapsedMilliseconds,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during inference: {ex.Message}");

                // Return error response
                return new ActionMessage
                {
                    HeadingDeg = request.AircraftHeadingDeg,
                    AltitudeFt = request.AircraftAltitudeFt,
                    SpeedKts = request.AircraftSpeedKts,
                    Confidence = 0,
                    Command = "ERROR: Could not generate command",
                    InferenceTimeMs = 0,
                };
            }
        }

        /// <summary>
        /// Generate ATC command from action
        /// </summary>
        private string GenerateATCCommand(MLAction action, GameObservation observation)
        {
            var commands = new List<string>();

            // Heading command
            float headingDiff = Math.Abs(action.HeadingDeg - observation.AircraftHeadingDeg);
            if (headingDiff > 2) // More than 2 degree difference
            {
                if (action.HeadingDeg > observation.AircraftHeadingDeg)
                {
                    commands.Add($"turn right heading {action.HeadingDeg:F0}");
                }
                else
                {
                    commands.Add($"turn left heading {action.HeadingDeg:F0}");
                }
            }

            // Altitude command
            float altitudeDiff = Math.Abs(action.AltitudeFt - observation.AircraftAltitudeFt);
            if (altitudeDiff > 100) // More than 100 ft difference
            {
                if (action.AltitudeFt > observation.AircraftAltitudeFt)
                {
                    commands.Add($"climb to {action.AltitudeFt:F0}");
                }
                else
                {
                    commands.Add($"descend to {action.AltitudeFt:F0}");
                }
            }

            // Speed command
            float speedDiff = Math.Abs(action.SpeedKts - observation.AircraftSpeedKts);
            if (speedDiff > 5) // More than 5 knot difference
            {
                if (action.SpeedKts > observation.AircraftSpeedKts)
                {
                    commands.Add($"increase speed to {action.SpeedKts:F0} knots");
                }
                else
                {
                    commands.Add($"reduce speed to {action.SpeedKts:F0} knots");
                }
            }

            // If no major changes, suggest maintain
            if (commands.Count == 0)
            {
                commands.Add("maintain current state");
            }

            // Combine commands
            return string.Join(" and ", commands);
        }

        /// <summary>
        /// Get health status
        /// </summary>
        public Dictionary<string, object> GetStatus()
        {
            return new Dictionary<string, object>
            {
                { "IsRunning", IsRunning },
                { "Port", _port },
                { "ModelStats", _modelService.GetPerformanceStats() },
                { "Timestamp", DateTime.UtcNow.ToString("O") },
            };
        }

        /// <summary>
        /// Get service definition for gRPC
        /// </summary>
        private object GetServiceDefinition()
        {
            // This would be generated from .proto file
            // For demonstration, returning placeholder
            return null;
        }
    }

    /// <summary>
    /// Client for AI Agent gRPC service
    /// </summary>
    public class AIAgentGrpcClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger<AIAgentGrpcClient> _logger;

        public AIAgentGrpcClient(
            string host,
            int port,
            ILogger<AIAgentGrpcClient> logger)
        {
            _host = host ?? "localhost";
            _port = port;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Call inference on remote service
        /// </summary>
        public async Task<ActionMessage> InferAsync(ObservationMessage observation)
        {
            try
            {
                var channel = new Channel($"{_host}:{_port}", ChannelCredentials.Insecure);
                await channel.ConnectAsync(timeout: TimeSpan.FromSeconds(5));

                _logger.LogDebug($"Connected to AI Agent service at {_host}:{_port}");

                // In a real implementation, would call the gRPC stub here
                // For demonstration purposes, returning null
                await channel.ShutdownAsync();

                return new ActionMessage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling remote inference service: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get health status
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var channel = new Channel($"{_host}:{_port}", ChannelCredentials.Insecure);
                await channel.ConnectAsync(timeout: TimeSpan.FromSeconds(2));
                await channel.ShutdownAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
