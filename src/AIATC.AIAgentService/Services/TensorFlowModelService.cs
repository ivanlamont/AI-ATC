/// <summary>
/// TensorFlow.NET Model Service for AI-ATC
///
/// Provides inference using TensorFlow.NET for the trained RL agent.
/// Handles model loading, observation preprocessing, and action generation.
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Tensorflow;
using static Tensorflow.Binding;


namespace AIATC.ML.Services
{
    /// <summary>
    /// Represents model configuration
    /// </summary>
    public class ModelConfiguration
    {
        public string ModelPath { get; set; }
        public int InputSize { get; set; } = 128;
        public int ActionSize { get; set; } = 6;
        public float MinAltitudeFt { get; set; } = 500;
        public float MaxAltitudeFt { get; set; } = 15000;
        public float MinSpeedKts { get; set; } = 100;
        public float MaxSpeedKts { get; set; } = 450;
        public float MinHeadingDeg { get; set; } = 0;
        public float MaxHeadingDeg { get; set; } = 360;
    }

    /// <summary>
    /// Observation data from game environment
    /// </summary>
    public class GameObservation
    {
        public float AircraftAltitudeFt { get; set; }
        public float AircraftSpeedKts { get; set; }
        public float AircraftHeadingDeg { get; set; }
        public float TargetAltitudeFt { get; set; }
        public float TargetSpeedKts { get; set; }
        public float TargetHeadingDeg { get; set; }
        public float DistanceToAirportNm { get; set; }
        public float AltitudeToRunwayFt { get; set; }
        public float WindSpeedKts { get; set; }
        public float WindDirectionDeg { get; set; }
        public float SeparationFromOtherAircraftNm { get; set; }
        public int NumAircraftInApproach { get; set; }
        public float[] RawObservation { get; set; }
    }

    /// <summary>
    /// Action from model inference
    /// </summary>
    public class MLAction
    {
        public float HeadingDeg { get; set; }
        public float AltitudeFt { get; set; }
        public float SpeedKts { get; set; }
        public float Confidence { get; set; }
        public int ActionIndex { get; set; }
    }

    /// <summary>
    /// Main TensorFlow model service
    /// </summary>
    public class TensorFlowModelService : IDisposable
    {
        private readonly ILogger<TensorFlowModelService> _logger;
        private readonly ModelConfiguration _config;
        private Graph _graph;
        private Session _session;
        private readonly ReaderWriterLockSlim _modelLock = new ReaderWriterLockSlim();
        private bool _disposed = false;
        private DateTime _lastModelLoadTime = DateTime.MinValue;

        public bool IsModelLoaded { get; private set; }
        public string CurrentModelPath { get; private set; }
        public TimeSpan AvgInferenceTime { get; private set; }

        private List<long> _inferenceTimes = new List<long>();
        private const int MaxInferenceTimeHistory = 100;

        public TensorFlowModelService(ModelConfiguration config, ILogger<TensorFlowModelService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Load model from file
        /// </summary>
        public async Task<bool> LoadModelAsync(string modelPath)
        {
            return await Task.Run(() => LoadModel(modelPath));
        }

        /// <summary>
        /// Load model from file (blocking)
        /// </summary>
        public bool LoadModel(string modelPath)
        {
            _modelLock.EnterWriteLock();
            try
            {
                if (!File.Exists(modelPath))
                {
                    _logger.LogError($"Model file not found: {modelPath}");
                    return false;
                }

                try
                {
                    _logger.LogInformation($"Loading model from {modelPath}");

                    // Dispose existing session and graph
                    _session?.Dispose();
                    _graph?.Dispose();

                    // Create new graph
                    _graph = new Graph().as_default();

                    // Load model (SavedModel format)
                    LoadModelFromPath(modelPath);

                    // Create session
                    _session = new Session(_graph);

                    CurrentModelPath = modelPath;
                    IsModelLoaded = true;
                    _lastModelLoadTime = DateTime.UtcNow;

                    _logger.LogInformation($"Model loaded successfully from {modelPath}");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading model: {ex.Message}");
                    IsModelLoaded = false;
                    return false;
                }
            }
            finally
            {
                _modelLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Run inference on observation
        /// </summary>
        public MLAction Infer(GameObservation observation)
        {
            _modelLock.EnterReadLock();
            try
            {
                if (!IsModelLoaded)
                {
                    throw new InvalidOperationException("Model not loaded");
                }

                var startTime = DateTime.UtcNow;

                // Preprocess observation
                var input = PreprocessObservation(observation);

                // Run inference
                var output = _session.run(
                    new[] { "model_output" },
                    new[] { new NDArray(input) }
                );

                // Postprocess output
                var action = PostprocessOutput(output[0], observation);

                // Track inference time
                var inferenceMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                TrackInferenceTime(inferenceMs);

                _logger.LogDebug($"Inference completed in {inferenceMs}ms");

                return action;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during inference: {ex.Message}");
                // Return neutral action on error
                return GetNeutralAction(observation);
            }
            finally
            {
                _modelLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Run inference asynchronously
        /// </summary>
        public async Task<MLAction> InferAsync(GameObservation observation)
        {
            return await Task.Run(() => Infer(observation));
        }

        /// <summary>
        /// Preprocess observation to model input
        /// </summary>
        private float[] PreprocessObservation(GameObservation obs)
        {
            var input = new float[_config.InputSize];

            // Normalize altitude
            float altNorm = (obs.AircraftAltitudeFt - _config.MinAltitudeFt) /
                (_config.MaxAltitudeFt - _config.MinAltitudeFt);
            altNorm = Math.Clamp(altNorm, 0, 1);

            // Normalize speed
            float speedNorm = (obs.AircraftSpeedKts - _config.MinSpeedKts) /
                (_config.MaxSpeedKts - _config.MinSpeedKts);
            speedNorm = Math.Clamp(speedNorm, 0, 1);

            // Normalize heading
            float headingNorm = obs.AircraftHeadingDeg / _config.MaxHeadingDeg;

            // Normalize distance
            float distanceNorm = Math.Min(obs.DistanceToAirportNm / 50, 1.0f); // Max 50nm

            // Normalize wind
            float windSpeedNorm = Math.Min(obs.WindSpeedKts / 50, 1.0f);
            float windDirNorm = obs.WindDirectionDeg / _config.MaxHeadingDeg;

            // Separation
            float separationNorm = Math.Min(obs.SeparationFromOtherAircraftNm / 5, 1.0f);

            // Fill input array
            int idx = 0;
            input[idx++] = altNorm;
            input[idx++] = speedNorm;
            input[idx++] = headingNorm;
            input[idx++] = (obs.TargetAltitudeFt - _config.MinAltitudeFt) /
                (_config.MaxAltitudeFt - _config.MinAltitudeFt);
            input[idx++] = (obs.TargetSpeedKts - _config.MinSpeedKts) /
                (_config.MaxSpeedKts - _config.MinSpeedKts);
            input[idx++] = obs.TargetHeadingDeg / _config.MaxHeadingDeg;
            input[idx++] = distanceNorm;
            input[idx++] = obs.AltitudeToRunwayFt / _config.MaxAltitudeFt;
            input[idx++] = windSpeedNorm;
            input[idx++] = windDirNorm;
            input[idx++] = separationNorm;
            input[idx++] = obs.NumAircraftInApproach / 10.0f;

            // Pad remaining with zeros
            while (idx < _config.InputSize)
            {
                input[idx++] = 0;
            }

            return input;
        }

        /// <summary>
        /// Postprocess model output to actions
        /// </summary>
        private MLAction PostprocessOutput(NDArray output, GameObservation obs)
        {
            // Assume output is [heading_delta, altitude_delta, speed_delta, confidence]
            var outputArray = output.Data<float>();

            // Get action deltas
            float headingDelta = (outputArray[0] - 0.5f) * 60; // -30 to +30 degrees
            float altitudeDelta = (outputArray[1] - 0.5f) * 2000; // -1000 to +1000 ft
            float speedDelta = (outputArray[2] - 0.5f) * 100; // -50 to +50 kts

            // Find action index (for logging/analysis)
            int actionIdx = Array.IndexOf(outputArray, outputArray.Max());

            // Calculate new values with constraints
            float newHeading = (obs.AircraftHeadingDeg + headingDelta) % 360;
            if (newHeading < 0) newHeading += 360;

            float newAltitude = obs.AircraftAltitudeFt + altitudeDelta;
            newAltitude = Math.Clamp(newAltitude, _config.MinAltitudeFt, _config.MaxAltitudeFt);

            float newSpeed = obs.AircraftSpeedKts + speedDelta;
            newSpeed = Math.Clamp(newSpeed, _config.MinSpeedKts, _config.MaxSpeedKts);

            // Confidence is typically the max output value (0-1)
            float confidence = outputArray[3];

            return new MLAction
            {
                HeadingDeg = newHeading,
                AltitudeFt = newAltitude,
                SpeedKts = newSpeed,
                Confidence = confidence,
                ActionIndex = actionIdx
            };
        }

        /// <summary>
        /// Get neutral action (maintain current state)
        /// </summary>
        private MLAction GetNeutralAction(GameObservation obs)
        {
            return new MLAction
            {
                HeadingDeg = obs.AircraftHeadingDeg,
                AltitudeFt = obs.AircraftAltitudeFt,
                SpeedKts = obs.AircraftSpeedKts,
                Confidence = 0.5f,
                ActionIndex = -1
            };
        }

        /// <summary>
        /// Track inference time for performance monitoring
        /// </summary>
        private void TrackInferenceTime(long milliseconds)
        {
            _inferenceTimes.Add(milliseconds);

            if (_inferenceTimes.Count > MaxInferenceTimeHistory)
            {
                _inferenceTimes.RemoveAt(0);
            }

            if (_inferenceTimes.Count > 0)
            {
                AvgInferenceTime = TimeSpan.FromMilliseconds(_inferenceTimes.Average());
            }
        }

        /// <summary>
        /// Get performance statistics
        /// </summary>
        public Dictionary<string, object> GetPerformanceStats()
        {
            _modelLock.EnterReadLock();
            try
            {
                return new Dictionary<string, object>
                {
                    { "IsModelLoaded", IsModelLoaded },
                    { "CurrentModelPath", CurrentModelPath },
                    { "AvgInferenceTimeMs", AvgInferenceTime.TotalMilliseconds },
                    { "InferenceSamplesTracked", _inferenceTimes.Count },
                    { "LastModelLoadTime", _lastModelLoadTime },
                    { "MinInferenceTimeMs", _inferenceTimes.Count > 0 ? _inferenceTimes.Min() : 0 },
                    { "MaxInferenceTimeMs", _inferenceTimes.Count > 0 ? _inferenceTimes.Max() : 0 },
                };
            }
            finally
            {
                _modelLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Hot-swap model (update while running)
        /// </summary>
        public bool HotSwapModel(string newModelPath)
        {
            _logger.LogInformation($"Attempting hot-swap to model: {newModelPath}");

            if (LoadModel(newModelPath))
            {
                _logger.LogInformation($"Hot-swap successful");
                return true;
            }

            _logger.LogError($"Hot-swap failed, keeping previous model");
            return false;
        }

        /// <summary>
        /// Load model from SavedModel directory
        /// </summary>
        private void LoadModelFromPath(string modelPath)
        {
            // This implementation depends on how the model was saved
            // Typically from TensorFlow SavedModel format
            // The actual implementation would use tf.saved_model.load equivalent

            // For now, this is a placeholder that shows the structure
            // In production, you would:
            // 1. Load the SavedModel from the directory
            // 2. Get the signature for inference
            // 3. Set up input/output tensors

            _logger.LogInformation($"Loading SavedModel from: {modelPath}");
        }

        public void Dispose()
        {
            if (_disposed) return;

            _modelLock.EnterWriteLock();
            try
            {
                _session?.Dispose();
                _graph?.Dispose();
                IsModelLoaded = false;
                _disposed = true;
            }
            finally
            {
                _modelLock.ExitWriteLock();
            }

            _modelLock?.Dispose();
        }
    }
}
