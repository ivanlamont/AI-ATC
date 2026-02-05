# ML Model Porting: Python to C# with TensorFlow.NET

## Overview

This document describes the porting of the Python reinforcement learning (RL) agent for AI-ATC to C# using TensorFlow.NET. The AI Agent provides real-time ATC command generation based on game observations.

## Architecture

### Service Components

The ML inference service consists of two main C# components:

1. **TensorFlowModelService** - Core inference engine
2. **AIAgentGrpcService** - Network interface for remote inference

### Data Flow

```
Game Observation
    ↓
[TensorFlowModelService.Infer()]
    ↓
[PreprocessObservation]
    ↓
[TensorFlow Model Inference]
    ↓
[PostprocessOutput]
    ↓
[AIAgentGrpcService.GenerateATCCommand()]
    ↓
ATC Command Message
```

## TensorFlowModelService

### Responsibility

Handles all model-related operations:
- Model loading and lifecycle management
- Observation preprocessing
- TensorFlow inference execution
- Action postprocessing
- Performance monitoring
- Hot-swapping models at runtime

### Key Features

#### Thread-Safe Operations

The service uses `ReaderWriterLockSlim` to ensure safe concurrent access:

```csharp
_modelLock.EnterReadLock();
try
{
    // Inference operations
    var output = _session.run(new[] { "model_output" }, new[] { new NDArray(input) });
}
finally
{
    _modelLock.ExitReadLock();
}
```

**Why**: Multiple game threads may request inference simultaneously while the service might be hot-swapping the model.

#### Model Loading

```csharp
public bool LoadModel(string modelPath)
{
    // 1. Validate file exists
    if (!File.Exists(modelPath)) return false;

    // 2. Dispose old resources
    _session?.Dispose();
    _graph?.Dispose();

    // 3. Create new graph and load model
    _graph = new Graph().as_default();
    LoadModelFromPath(modelPath);

    // 4. Create session
    _session = new Session(_graph);

    // 5. Update metadata
    IsModelLoaded = true;
    CurrentModelPath = modelPath;
}
```

Models are expected in **TensorFlow SavedModel format**, which is the standard export format from Python.

#### Observation Preprocessing

The preprocessing pipeline converts game observations to normalized input vectors (0-1 range).

**Python Reference** (for comparison):

```python
def preprocess_observation(obs):
    # Normalize altitude
    alt_norm = (obs.altitude_ft - MIN_ALT) / (MAX_ALT - MIN_ALT)
    alt_norm = np.clip(alt_norm, 0, 1)

    # Normalize speed
    speed_norm = (obs.speed_kts - MIN_SPEED) / (MAX_SPEED - MIN_SPEED)
    speed_norm = np.clip(speed_norm, 0, 1)

    # ... additional normalizations
    return normalized_input
```

**C# Implementation**:

```csharp
private float[] PreprocessObservation(GameObservation obs)
{
    var input = new float[_config.InputSize]; // 128 values

    // Altitude: 500-15000 ft → 0-1
    float altNorm = (obs.AircraftAltitudeFt - _config.MinAltitudeFt) /
                    (_config.MaxAltitudeFt - _config.MinAltitudeFt);
    altNorm = Math.Clamp(altNorm, 0, 1);
    input[0] = altNorm;

    // Speed: 100-450 kts → 0-1
    float speedNorm = (obs.AircraftSpeedKts - _config.MinSpeedKts) /
                      (_config.MaxSpeedKts - _config.MinSpeedKts);
    speedNorm = Math.Clamp(speedNorm, 0, 1);
    input[1] = speedNorm;

    // ... continue with other fields

    // Pad to input size with zeros
    while (idx < _config.InputSize)
        input[idx++] = 0;

    return input;
}
```

**Normalization Formula**:
```
normalized_value = (value - min) / (max - min)
clamped_value = clamp(normalized_value, 0, 1)
```

**Fields Preprocessed** (12 primary + padding to 128):

| Index | Field | Min | Max | Purpose |
|-------|-------|-----|-----|---------|
| 0 | Aircraft Altitude | 500 ft | 15,000 ft | Current altitude |
| 1 | Aircraft Speed | 100 kts | 450 kts | Current speed |
| 2 | Aircraft Heading | 0° | 360° | Current direction |
| 3 | Target Altitude | 500 ft | 15,000 ft | Assigned altitude |
| 4 | Target Speed | 100 kts | 450 kts | Assigned speed |
| 5 | Target Heading | 0° | 360° | Assigned direction |
| 6 | Distance to Airport | 0 nm | 50 nm | Approach distance |
| 7 | Altitude to Runway | 0 ft | 15,000 ft | Vertical separation |
| 8 | Wind Speed | 0 kts | 50 kts | Wind magnitude |
| 9 | Wind Direction | 0° | 360° | Wind origin |
| 10 | Aircraft Separation | 0 nm | 5 nm | Distance from other aircraft |
| 11 | Aircraft in Approach | 0 | 10 | Number of aircraft |

#### Action Postprocessing

Model output (6 values) is converted to constrained actions:

```csharp
private MLAction PostprocessOutput(NDArray output, GameObservation obs)
{
    var outputArray = output.Data<float>();

    // Output format: [heading_delta, altitude_delta, speed_delta, confidence, ?, ?]
    // Values are normalized to 0-1 range, we convert to deltas

    float headingDelta = (outputArray[0] - 0.5f) * 60;  // -30 to +30 degrees
    float altitudeDelta = (outputArray[1] - 0.5f) * 2000; // -1000 to +1000 ft
    float speedDelta = (outputArray[2] - 0.5f) * 100;    // -50 to +50 kts

    // Calculate new values
    float newHeading = (obs.AircraftHeadingDeg + headingDelta) % 360;
    float newAltitude = Math.Clamp(obs.AircraftAltitudeFt + altitudeDelta,
        _config.MinAltitudeFt, _config.MaxAltitudeFt);
    float newSpeed = Math.Clamp(obs.AircraftSpeedKts + speedDelta,
        _config.MinSpeedKts, _config.MaxSpeedKts);

    // Confidence from model
    float confidence = outputArray[3];

    return new MLAction
    {
        HeadingDeg = newHeading,
        AltitudeFt = newAltitude,
        SpeedKts = newSpeed,
        Confidence = confidence,
        ActionIndex = Array.IndexOf(outputArray, outputArray.Max())
    };
}
```

**Conversion Details**:

1. **Heading Delta**: Model outputs 0-1 → rescale to -30 to +30 degrees
2. **Altitude Delta**: Model outputs 0-1 → rescale to -1000 to +1000 feet
3. **Speed Delta**: Model outputs 0-1 → rescale to -50 to +50 knots
4. **Constraints**: Applied using `Math.Clamp()` to ensure values stay in valid ranges

#### Performance Monitoring

The service tracks inference time for performance analysis:

```csharp
private void TrackInferenceTime(long milliseconds)
{
    _inferenceTimes.Add(milliseconds);

    // Keep last 100 samples
    if (_inferenceTimes.Count > MaxInferenceTimeHistory)
        _inferenceTimes.RemoveAt(0);

    // Calculate average
    if (_inferenceTimes.Count > 0)
        AvgInferenceTime = TimeSpan.FromMilliseconds(_inferenceTimes.Average());
}
```

**Statistics Available** via `GetPerformanceStats()`:
- Average inference time (ms)
- Min/max inference time
- Number of samples tracked
- Last model load time
- Model path

#### Hot-Swapping

Models can be swapped while the service is running:

```csharp
public bool HotSwapModel(string newModelPath)
{
    _logger.LogInformation($"Attempting hot-swap to: {newModelPath}");

    if (LoadModel(newModelPath))
    {
        _logger.LogInformation("Hot-swap successful");
        return true;
    }

    _logger.LogError("Hot-swap failed, keeping previous model");
    return false;
}
```

**Use Case**: Deploy improved models without restarting the service.

### Model Configuration

```csharp
public class ModelConfiguration
{
    public string ModelPath { get; set; }
    public int InputSize { get; set; } = 128;    // Input vector size
    public int ActionSize { get; set; } = 6;     // Output vector size
    public float MinAltitudeFt { get; set; } = 500;
    public float MaxAltitudeFt { get; set; } = 15000;
    public float MinSpeedKts { get; set; } = 100;
    public float MaxSpeedKts { get; set; } = 450;
    public float MinHeadingDeg { get; set; } = 0;
    public float MaxHeadingDeg { get; set; } = 360;
}
```

Must match the normalization ranges used during Python training.

## AIAgentGrpcService

### Responsibility

Provides gRPC interface for remote inference requests and ATC command generation.

### gRPC Service Definition

The service is defined in `Protos/ai_agent.proto`:

```protobuf
service AIAgent {
  rpc Infer (ObservationMessage) returns (ActionMessage);
  rpc HealthCheck (HealthCheckRequest) returns (HealthCheckResponse);
  rpc GetStatus (StatusRequest) returns (StatusResponse);
}
```

### Message Types

#### ObservationMessage

Contains game state to be processed:

```csharp
public class ObservationMessage
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
}
```

#### ActionMessage

Contains AI-generated command:

```csharp
public class ActionMessage
{
    public float HeadingDeg { get; set; }       // New heading
    public float AltitudeFt { get; set; }       // New altitude
    public float SpeedKts { get; set; }         // New speed
    public float Confidence { get; set; }       // Confidence (0-1)
    public string Command { get; set; }         // Natural language ATC command
    public long InferenceTimeMs { get; set; }   // Model execution time
}
```

### ATC Command Generation

The service converts ML actions to natural language ATC commands:

```csharp
private string GenerateATCCommand(MLAction action, GameObservation observation)
{
    var commands = new List<string>();

    // Heading command (if difference > 2 degrees)
    float headingDiff = Math.Abs(action.HeadingDeg - observation.AircraftHeadingDeg);
    if (headingDiff > 2)
    {
        if (action.HeadingDeg > observation.AircraftHeadingDeg)
            commands.Add($"turn right heading {action.HeadingDeg:F0}");
        else
            commands.Add($"turn left heading {action.HeadingDeg:F0}");
    }

    // Altitude command (if difference > 100 feet)
    float altitudeDiff = Math.Abs(action.AltitudeFt - observation.AircraftAltitudeFt);
    if (altitudeDiff > 100)
    {
        if (action.AltitudeFt > observation.AircraftAltitudeFt)
            commands.Add($"climb to {action.AltitudeFt:F0}");
        else
            commands.Add($"descend to {action.AltitudeFt:F0}");
    }

    // Speed command (if difference > 5 knots)
    float speedDiff = Math.Abs(action.SpeedKts - observation.AircraftSpeedKts);
    if (speedDiff > 5)
    {
        if (action.SpeedKts > observation.AircraftSpeedKts)
            commands.Add($"increase speed to {action.SpeedKts:F0} knots");
        else
            commands.Add($"reduce speed to {action.SpeedKts:F0} knots");
    }

    // Default
    if (commands.Count == 0)
        commands.Add("maintain current state");

    return string.Join(" and ", commands);
}
```

**Command Thresholds**:
- Heading: > 2° change
- Altitude: > 100 ft change
- Speed: > 5 kt change

**Command Format Examples**:
- "turn right heading 270"
- "climb to 5000" and "reduce speed to 200 knots"
- "turn left heading 090" and "descend to 2000"
- "maintain current state"

### Server Lifecycle

```csharp
public async Task StartAsync()
{
    _server = new Server
    {
        Services = { GetServiceDefinition() },
        Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
    };

    await _server.StartAsync();
    IsRunning = true;
}

public async Task StopAsync()
{
    if (_server == null) return;
    await _server.ShutdownAsync();
    IsRunning = false;
}
```

### Client Usage

For calling the remote service:

```csharp
var client = new AIAgentGrpcClient("localhost", 50051, logger);

var observation = new ObservationMessage
{
    AircraftAltitudeFt = 5000,
    AircraftSpeedKts = 250,
    // ... other fields
};

var action = await client.InferAsync(observation);
Console.WriteLine(action.Command); // "turn right heading 270"
```

## Integration with Game

### Usage Pattern

1. **Initialize**:
```csharp
var config = new ModelConfiguration
{
    ModelPath = "/models/ai_agent.pb",
    InputSize = 128,
    ActionSize = 6
};

var modelService = new TensorFlowModelService(config, logger);
modelService.LoadModel(config.ModelPath);

var grpcService = new AIAgentGrpcService(modelService, logger);
await grpcService.StartAsync();
```

2. **Per Game Tick**:
```csharp
var observation = new ObservationMessage
{
    AircraftAltitudeFt = aircraft.AltitudeFt,
    AircraftSpeedKts = aircraft.SpeedKts,
    // ... populate all fields
};

var action = grpcService.Infer(observation);
Console.WriteLine($"Command: {action.Command}");
Console.WriteLine($"Confidence: {action.Confidence:P}");
```

3. **Shutdown**:
```csharp
await grpcService.StopAsync();
modelService.Dispose();
```

## Dependencies

Required NuGet packages:

```xml
<PackageReference Include="Grpc.AspNetCore" Version="2.76.0" />
<PackageReference Include="Grpc.Tools" Version="2.68.0" />
<PackageReference Include="SciSharp.TensorFlow.Redist" Version="2.16.0" />
```

## Python Model Export

### Standard Workflow

1. **Train in Python**:
```python
import tensorflow as tf

# Train your RL agent
agent = create_agent()
agent.train(episodes=1000)

# Save to SavedModel format
agent.model.save("/path/to/saved_model")
```

2. **Export Signatures**:
```python
# Ensure model has proper serving signature
def concrete_func(observation):
    return agent.policy(observation)

concrete_func = tf.function(
    concrete_func,
    input_signature=[tf.TensorSpec([None, 128], tf.float32)]
)

# Save with serving signature
agent.model.save(
    "/path/to/saved_model",
    save_format="tf",
    signatures=concrete_func
)
```

3. **Verify Model Structure**:
```bash
saved_model_cli show --dir /path/to/saved_model --all
```

### Model Format Requirements

- **Format**: TensorFlow SavedModel (directory with `saved_model.pb` and `variables/`)
- **Input Shape**: [batch_size, 128] (float32)
- **Output Shape**: [batch_size, 6] (float32)
- **Input Name**: "model_input" or configure in code
- **Output Name**: "model_output" or configure in code

## Differences from Python

| Aspect | Python | C# |
|--------|--------|-----|
| **Type System** | Dynamic | Static |
| **Concurrency** | GIL-limited | Native threading |
| **Memory** | Automatic GC | Automatic GC |
| **Performance** | ~10-50ms | ~5-20ms |
| **Distribution** | Pickle + file | SavedModel |

### Performance Implications

1. **C# is typically faster**: 2-5x speedup due to compiled code
2. **No GIL contention**: Genuine multi-threading on multiple cores
3. **Memory overhead**: SavedModel format may be slightly larger
4. **Inference latency**: Expected 5-20ms for single observations

## Troubleshooting

### Model Won't Load

```
Error loading model: /path/to/model - File not found
```

**Solution**: Verify path and ensure SavedModel format with `saved_model.pb` present.

### Poor Inference Quality

**Causes**:
- Normalization ranges don't match training
- Model expects different input format
- Output postprocessing incorrect

**Debug**:
```csharp
// Log normalized input
var preprocessed = PreprocessObservation(obs);
_logger.LogDebug($"Normalized input: {string.Join(",", preprocessed.Take(12))}");

// Log raw output
var output = _session.run(...);
_logger.LogDebug($"Raw output: {string.Join(",", output[0].Data<float>())}");
```

### gRPC Connection Failures

**Solutions**:
- Ensure server started with `StartAsync()`
- Verify port not in use
- Check firewall/network settings
- Use `HealthCheckAsync()` to diagnose

### Memory Leaks

**Prevention**:
- Call `Dispose()` on service shutdown
- Don't create multiple `Session` instances
- Use `using` statement for resource cleanup

## Testing

Comprehensive unit tests are provided in:
- `AIATC.AIAgentService.Tests/TensorFlowModelServiceTests.cs`
- `AIATC.AIAgentService.Tests/AIAgentGrpcServiceTests.cs`

Tests cover:
- Configuration validation
- Model loading/unloading
- Observation preprocessing
- Action postprocessing
- Command generation
- Error handling
- Server lifecycle
- Performance monitoring

## Future Enhancements

1. **Quantization**: Reduce model size for edge deployment
2. **Batch Inference**: Process multiple observations in parallel
3. **Model Versioning**: Support multiple models with automatic fallback
4. **Metrics Export**: Prometheus integration for monitoring
5. **Distributed Inference**: Load balancing across multiple services

## References

- [TensorFlow.NET Documentation](https://github.com/SciSharp/TensorFlow.NET)
- [gRPC C# Guide](https://grpc.io/docs/languages/csharp/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)
- [TensorFlow SavedModel Format](https://www.tensorflow.org/guide/saved_model)
