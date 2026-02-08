# AI-ATC Microservices Architecture

## Overview

The AI-ATC system is built using a microservices architecture with the following principles:
- **Service Independence**: Each service can be deployed, scaled, and updated independently
- **gRPC Communication**: High-performance RPC for service-to-service communication
- **DAPR Integration**: Service discovery, pub/sub, state management, and observability
- **Event-Driven**: Asynchronous communication via pub/sub for non-blocking operations
- **Domain-Driven Design**: Services organized around business capabilities

---

## Service Catalog

### 1. AIATC.SimulationService
**Purpose:** Core ATC simulation engine

**Responsibilities:**
- Aircraft physics simulation (position, altitude, speed, heading)
- Wind and weather effects on aircraft
- Separation monitoring and violation detection
- Time step management and simulation clock
- Real-time state broadcasting

**Technology:**
- ASP.NET Core Web API
- SignalR for real-time updates to clients
- In-memory state with snapshot capability

**gRPC Services:**
- `SimulationService.proto`:
  - `CreateSimulation(SimulationConfig) -> SimulationId`
  - `ApplyCommand(SimulationId, AircraftId, Command) -> CommandResult`
  - `GetSimulationState(SimulationId) -> SimulationState`
  - `AdvanceTime(SimulationId, DeltaTime) -> SimulationState`

**Pub/Sub Topics:**
- Publishes: `simulation.aircraft-moved`, `simulation.separation-violation`, `simulation.aircraft-landed`
- Subscribes: `command.atc-clearance`

**State Management:**
- DAPR state store for simulation snapshots (save/load)

---

### 2. AIATC.ScenarioService
**Purpose:** Scenario and configuration management

**Responsibilities:**
- Load and save scenario definitions
- Manage scenario library (pre-built challenges)
- Handle custom scenario creation
- Fetch live aircraft data from ADSBexchange
- Weather data integration (live METAR/TAF)
- Airport/runway/fix database

**Technology:**
- ASP.NET Core Web API
- Entity Framework Core with PostgreSQL

**gRPC Services:**
- `ScenarioService.proto`:
  - `GetScenario(ScenarioId) -> Scenario`
  - `ListScenarios(Filter) -> ScenarioList`
  - `CreateScenario(ScenarioDefinition) -> ScenarioId`
  - `GetLiveAircraftData(Location, Radius) -> AircraftList`
  - `GetWeatherData(AirportCode) -> WeatherData`

**Pub/Sub Topics:**
- Publishes: `scenario.loaded`, `scenario.weather-updated`

**Database Tables:**
- Scenarios, Airports, Runways, Fixes, Procedures, Weather

---

### 3. AIATC.UserService
**Purpose:** User authentication and profile management

**Responsibilities:**
- OAuth2 authentication flow
- JWT token generation and validation
- User profile management
- Session tracking
- Score history and statistics
- Guest user management

**Technology:**
- ASP.NET Core Web API
- Identity Server or Auth0 integration
- Entity Framework Core with PostgreSQL

**gRPC Services:**
- `UserService.proto`:
  - `Authenticate(Credentials) -> AuthToken`
  - `ValidateToken(Token) -> UserClaims`
  - `GetUserProfile(UserId) -> Profile`
  - `UpdateProfile(UserId, Profile) -> Result`
  - `RecordScore(UserId, ScenarioId, Score) -> Result`
  - `GetScoreHistory(UserId) -> ScoreList`

**Pub/Sub Topics:**
- Publishes: `user.authenticated`, `user.score-recorded`

**Database Tables:**
- Users, Sessions, Scores, Achievements

---

### 4. AIATC.AIAgentService
**Purpose:** AI agent inference and decision making

**Responsibilities:**
- Load trained TensorFlow.NET models
- Perform inference for AI decisions
- Generate ATC commands from AI policy
- Track AI agent performance metrics
- Model versioning and A/B testing

**Technology:**
- ASP.NET Core Web API
- TensorFlow.NET for model inference
- Model artifacts storage (Azure Blob / S3)

**gRPC Services:**
- `AIAgentService.proto`:
  - `GetAction(SimulationState) -> ATCCommand`
  - `LoadModel(ModelId) -> Result`
  - `GetModelMetrics(ModelId) -> Metrics`

**Pub/Sub Topics:**
- Subscribes: `simulation.aircraft-moved` (for autonomous agent scenarios)
- Publishes: `ai.command-issued`

**State Management:**
- Model artifacts in blob storage
- Inference metrics in time-series database

---

### 5. AIATC.AudioService
**Purpose:** Speech processing (STT/TTS) and audio streaming

**Responsibilities:**
- Receive audio stream from browser
- Speech-to-text transcription
- ATC command extraction from transcription
- Text-to-speech synthesis with accents
- Pilot voice selection based on airline
- Audio response delivery

**Technology:**
- ASP.NET Core Web API
- Azure Speech Services / Google Cloud Speech / AWS Transcribe
- WebSocket for audio streaming

**gRPC Services:**
- `AudioService.proto`:
  - `TranscribeAudio(AudioStream) -> Transcription`
  - `SynthesizeSpeech(Text, VoiceProfile) -> AudioStream`
  - `GetVoiceProfile(Airline) -> VoiceProfile`

**Pub/Sub Topics:**
- Subscribes: `command.atc-clearance` (to generate pilot responses)
- Publishes: `audio.transcription-complete`, `audio.response-ready`

---

### 6. AIATC.Web (Blazor WebAssembly)
**Purpose:** User interface and frontend application

**Responsibilities:**
- Radar display rendering
- User input handling (voice, text, mouse)
- Real-time visualization updates
- Scenario selection UI
- Leaderboard and profile views
- Challenge mode split-screen UI
- Admin dashboard for super-users

**Technology:**
- Blazor WebAssembly
- SignalR client for real-time updates
- Canvas/WebGL for radar rendering

**Communication:**
- REST APIs to all backend services
- SignalR connection to SimulationService
- WebSocket to AudioService for audio streaming

---

## Communication Patterns

### Synchronous (gRPC)
Used for:
- Command/query operations requiring immediate response
- Service-to-service request/reply
- Scenarios where caller needs to know outcome

Example: Web -> SimulationService.ApplyCommand()

### Asynchronous (Pub/Sub via DAPR)
Used for:
- Event notifications
- Fire-and-forget operations
- Decoupling services
- Broadcasting state changes

Example: SimulationService publishes `aircraft-moved` event, multiple services subscribe

---

## DAPR Configuration

### Components

**State Store:** Redis
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  metadata:
  - name: redisHost
    value: redis:6379
  - name: redisPassword
    value: ""
```

**Pub/Sub:** Redis Streams
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
spec:
  type: pubsub.redis
  metadata:
  - name: redisHost
    value: redis:6379
```

**Service Invocation:** Built-in
- Uses mDNS for local development
- Uses Kubernetes DNS in production

---

## Service Dependencies

```
AIATC.Web (Frontend)
  ├─> AIATC.SimulationService (gRPC + SignalR)
  ├─> AIATC.ScenarioService (gRPC)
  ├─> AIATC.UserService (gRPC)
  ├─> AIATC.AudioService (WebSocket)
  └─> AIATC.AIAgentService (Challenge mode)

AIATC.SimulationService
  ├─> AIATC.ScenarioService (load scenarios)
  └─> DAPR State Store (snapshots)

AIATC.AIAgentService
  └─> AIATC.SimulationService (get state, issue commands)

AIATC.AudioService
  └─> AIATC.SimulationService (command validation)

AIATC.ScenarioService
  └─> External APIs (ADSBexchange, METAR)
```

---

## Data Flow Examples

### Example 1: User Issues Voice Command

1. **User** speaks into microphone (Web)
2. **Web** streams audio via WebSocket to **AudioService**
3. **AudioService** transcribes speech to text
4. **AudioService** publishes `audio.transcription-complete` event
5. **Web** receives transcription, parses ATC command
6. **Web** calls **SimulationService.ApplyCommand()** via gRPC
7. **SimulationService** validates and applies command
8. **SimulationService** publishes `command.atc-clearance` event
9. **AudioService** subscribes to event, generates pilot voice response
10. **AudioService** streams TTS audio back to **Web**
11. **SimulationService** broadcasts updated state via SignalR
12. **Web** updates radar display

### Example 2: AI Agent Competes in Challenge Mode

1. **User** selects challenge mode scenario
2. **Web** calls **ScenarioService.GetScenario()**
3. **Web** creates two parallel simulations in **SimulationService**
4. **SimulationService** publishes state updates for both
5. **AIAgentService** subscribes to AI simulation state
6. **AIAgentService** performs inference, generates command
7. **AIAgentService** calls **SimulationService.ApplyCommand()**
8. Both simulations progress in parallel
9. **Web** displays split-screen with both simulations
10. **SimulationService** calculates scores for both
11. Winner determined, results saved to **UserService**

---

## Deployment Model

### Development
- All services run locally via `dapr run`
- Redis in Docker container
- PostgreSQL in Docker container

### Kubernetes Production
- Each service deployed as separate Deployment
- DAPR sidecar injected via annotation
- Redis and PostgreSQL as managed services (Azure/AWS)
- Ingress controller routes traffic to Web frontend
- Internal services not exposed externally

---

## Security

### Authentication
- OAuth2/OIDC for user authentication (UserService)
- JWT tokens for API authorization
- Service-to-service: mTLS via DAPR

### Authorization
- Role-based access control (User, SuperUser)
- API Gateway validates JWT before routing
- Service mesh policies for internal communication

### Secrets
- DAPR Secrets component (Azure Key Vault / AWS Secrets Manager)
- No secrets in code or config files

---

## Observability

### Metrics
- Prometheus scrapes DAPR metrics
- Custom metrics via OpenTelemetry

### Tracing
- Distributed tracing via DAPR + Zipkin/Jaeger
- Correlation IDs across service calls

### Logging
- Structured logging to stdout
- Aggregated in Grafana Loki or ELK stack

---

## Scalability

### Horizontal Scaling
- **SimulationService**: Stateful, one pod per active simulation (use StatefulSet)
- **ScenarioService**: Stateless, scale based on CPU
- **UserService**: Stateless, scale based on request rate
- **AIAgentService**: Scale based on inference queue depth
- **AudioService**: Scale based on concurrent audio streams
- **Web**: Static files served via CDN

### Resource Limits
```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "100m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

---

## Next Steps

1. Implement gRPC proto definitions for each service
2. Set up DAPR local development environment
3. Implement service interfaces and stubs
4. Add health check endpoints
5. Configure observability stack
