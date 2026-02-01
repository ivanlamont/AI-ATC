using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Scenarios;
using AIATC.Domain.Models.Scoring;
using Microsoft.Extensions.Logging;
using AIATC.Common;

namespace AIATC.Domain.Services
{
    /// <summary>
    /// Challenge Mode Service - manages AI vs Human competitive gameplay
    ///
    /// Runs two identical simulations in parallel:
    /// - Left side: Player-controlled (human)
    /// - Right side: AI-controlled
    ///
    /// Both sides follow the same rules, timing, and scoring system.
    /// </summary>
    public class ChallengeModeService
    {
        private readonly ILogger<ChallengeModeService> _logger;
        private readonly SimulationEngine _userSimulation;
        private readonly SimulationEngine _aiSimulation;
        private readonly AIAgentService _aiAgent;

        public bool IsActive { get; private set; }
        public ChallengeState State { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }

        // Synchronized time between both sides
        public float SimulationTimeSeconds { get; private set; }
        public float RealTimeSeconds { get; private set; }
        public float TimeMultiplier { get; set; } = 1.0f;

        // Challenge metadata
        public string ChallengeId { get; private set; }
        public string ScenarioId { get; private set; }
        public Difficulty Difficulty { get; private set; }

        // Command tracking
        public List<ChallengeCommand> UserCommandHistory { get; private set; }
        public List<ChallengeCommand> AiCommandHistory { get; private set; }

        // AI decision tracking (for display)
        public List<AIPrediction> AIPredictions { get; private set; }
        public bool ShowAIPredictions { get; set; } = false;

        // Events
        public event EventHandler<ChallengeStateChangedEventArgs> OnStateChanged;
        public event EventHandler<SeparationViolationEventArgs> OnSeparationViolation;
        public event EventHandler<AircraftLandedEventArgs> OnAircraftLanded;

        public ChallengeModeService(
            AIAgentService aiAgent,
            ILogger<ChallengeModeService> logger)
        {
            _aiAgent = aiAgent ?? throw new ArgumentNullException(nameof(aiAgent));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _userSimulation = new SimulationEngine();
            _aiSimulation = new SimulationEngine();

            UserCommandHistory = new List<ChallengeCommand>();
            AiCommandHistory = new List<ChallengeCommand>();
            AIPredictions = new List<AIPrediction>();

            State = ChallengeState.NotStarted;
        }

        /// <summary>
        /// Initialize a challenge mode session with identical scenarios
        /// </summary>
        public bool InitializeChallenge(
            string scenarioId,
            Difficulty difficulty)
        {
            try
            {
                _logger.LogInformation($"Initializing challenge mode for scenario {scenarioId}");

                ScenarioId = scenarioId;
                Difficulty = difficulty;
                ChallengeId = Guid.NewGuid().ToString();

                // Load the scenario for both simulations
                var scenario = _userSimulation.ScenarioService.GetScenario(scenarioId);
                if (scenario == null)
                {
                    _logger.LogError($"Scenario {scenarioId} not found");
                    return false;
                }

                // Register scenarios with both simulation engines
                _userSimulation.ScenarioService.RegisterScenario(scenario);
                _aiSimulation.ScenarioService.RegisterScenario(scenario);

                // Start scenarios
                _userSimulation.ScenarioService.StartScenario(scenario.Metadata.Id);
                _aiSimulation.ScenarioService.StartScenario(scenario.Metadata.Id);

                // Add aircraft to both simulations
                // Note: In a real implementation, aircraft would be spawned by the scenario
                // For now, we'll add some dummy aircraft
                var userAircraft = CreateTestAircraft();
                var aiAircraft = CreateTestAircraft();

                _userSimulation.AddAircraft(userAircraft);
                _aiSimulation.AddAircraft(aiAircraft);

                State = ChallengeState.Ready;
                IsActive = false;

                _logger.LogInformation($"Challenge {ChallengeId} initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing challenge: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start the challenge competition
        /// </summary>
        public void StartChallenge()
        {
            if (State != ChallengeState.Ready)
            {
                _logger.LogWarning($"Cannot start challenge in state {State}");
                return;
            }

            StartTime = DateTime.UtcNow;
            State = ChallengeState.Running;
            IsActive = true;
            SimulationTimeSeconds = 0;
            RealTimeSeconds = 0;

            _logger.LogInformation($"Challenge {ChallengeId} started");
            OnStateChanged?.Invoke(this, new ChallengeStateChangedEventArgs(State));
        }

        /// <summary>
        /// Pause the challenge
        /// </summary>
        public void PauseChallenge()
        {
            if (State == ChallengeState.Running)
            {
                State = ChallengeState.Paused;
                OnStateChanged?.Invoke(this, new ChallengeStateChangedEventArgs(State));
            }
        }

        /// <summary>
        /// Resume the challenge
        /// </summary>
        public void ResumeChallenge()
        {
            if (State == ChallengeState.Paused)
            {
                State = ChallengeState.Running;
                OnStateChanged?.Invoke(this, new ChallengeStateChangedEventArgs(State));
            }
        }

        /// <summary>
        /// Update both simulations (called every frame)
        /// </summary>
        public void UpdateChallenge(float deltaTimeSeconds)
        {
            if (State != ChallengeState.Running)
                return;

            // Scale delta time by time multiplier
            float scaledDeltaTime = deltaTimeSeconds * TimeMultiplier;

            // Update accumulated times (synchronized for both sides)
            RealTimeSeconds += deltaTimeSeconds;
            SimulationTimeSeconds += scaledDeltaTime;

            // Update user simulation
            _userSimulation.Update(scaledDeltaTime);

            // Update AI simulation with AI-issued commands
            UpdateAiSimulation(scaledDeltaTime);

            // Check for end conditions
            CheckEndConditions();
        }

        /// <summary>
        /// Submit a human player command
        /// </summary>
        public void SubmitUserCommand(string commandText, AircraftModel targetAircraft)
        {
            if (State != ChallengeState.Running)
                return;

            var command = new ChallengeCommand
            {
                CommandText = commandText,
                IssuedByAI = false,
                Timestamp = SimulationTimeSeconds,
                TargetCallsign = targetAircraft?.Callsign ?? "UNKNOWN"
            };

            UserCommandHistory.Add(command);

            // Apply command to user's simulation
            ApplyCommandToAircraft(targetAircraft, commandText);

            _logger.LogDebug($"User command: {commandText} → {command.TargetCallsign}");
        }

        /// <summary>
        /// Get AI command for a specific aircraft in AI's simulation
        /// </summary>
        private AiCommandResult GetAiCommand(AircraftModel targetAircraft)
        {
            try
            {
                // Prepare observation from AI simulation state
                var observation = PrepareObservation(targetAircraft, _aiSimulation);

                // Get AI decision through agent service
                var action = _aiAgent.GetAction(observation);

                // Convert action to ATC command
                var command = ConvertActionToCommand(action, targetAircraft, observation);

                return new AiCommandResult
                {
                    Command = command,
                    Action = action,
                    Confidence = action.Confidence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting AI command: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update AI simulation with AI-issued commands
        /// </summary>
        private void UpdateAiSimulation(float deltaTime)
        {
            // Get current aircraft in AI simulation
            var aiAircraft = _aiSimulation.Aircraft.ToList();

            // Every 2 seconds (or as configured), request AI commands
            if (ShouldRequestAiCommand())
            {
                foreach (var aircraft in aiAircraft)
                {
                    var result = GetAiCommand(aircraft);
                    if (result != null)
                    {
                        // Record prediction for display
                        AIPredictions.Add(new AIPrediction
                        {
                            Timestamp = SimulationTimeSeconds,
                            Callsign = aircraft.Callsign,
                            Command = result.Command,
                            Confidence = result.Confidence
                        });

                        // Apply command to AI simulation
                        var challengeCommand = new ChallengeCommand
                        {
                            CommandText = result.Command,
                            IssuedByAI = true,
                            Timestamp = SimulationTimeSeconds,
                            TargetCallsign = aircraft.Callsign,
                            Confidence = result.Confidence
                        };

                        AiCommandHistory.Add(challengeCommand);
                        ApplyCommandToAircraft(aircraft, result.Command);

                        _logger.LogDebug($"AI command: {result.Command} → {aircraft.Callsign} (confidence: {result.Confidence:P})");
                    }
                }
            }
        }

        /// <summary>
        /// Check if challenge should end
        /// </summary>
        private void CheckEndConditions()
        {
            // Challenge ends when:
            // 1. Time limit exceeded (configurable, default 30 minutes)
            // 2. All aircraft landed or left airspace
            // 3. User or AI gives up

            bool userAllDone = _userSimulation.Aircraft.Count == 0;
            bool aiAllDone = _aiSimulation.Aircraft.Count == 0;
            bool timeLimitExceeded = SimulationTimeSeconds > 1800; // 30 minutes

            if ((userAllDone && aiAllDone) || timeLimitExceeded)
            {
                EndChallenge();
            }
        }

        /// <summary>
        /// End the challenge and determine winner
        /// </summary>
        public void EndChallenge()
        {
            if (State == ChallengeState.Running || State == ChallengeState.Paused)
            {
                State = ChallengeState.Completed;
                EndTime = DateTime.UtcNow;
                IsActive = false;

                var result = DetermineWinner();

                _logger.LogInformation($"Challenge {ChallengeId} completed. Winner: {result.Winner}");
                OnStateChanged?.Invoke(this, new ChallengeStateChangedEventArgs(State));
            }
        }

        /// <summary>
        /// Get current scores for both sides
        /// </summary>
        public ChallengeComparison GetCurrentComparison()
        {
            var userScenario = _userSimulation.ScenarioService.GetActiveScenario();
            var aiScenario = _aiSimulation.ScenarioService.GetActiveScenario();

            return new ChallengeComparison
            {
                UserScore = userScenario != null ? new SessionScore { TotalScore = userScenario.CurrentScore } : new SessionScore(),
                AiScore = aiScenario != null ? new SessionScore { TotalScore = aiScenario.CurrentScore } : new SessionScore(),
                UserAircraftCount = _userSimulation.Aircraft.Count,
                AiAircraftCount = _aiSimulation.Aircraft.Count,
                UserCommandCount = UserCommandHistory.Count,
                AiCommandCount = AiCommandHistory.Count,
                SimulationTimeSeconds = SimulationTimeSeconds
            };
        }

        /// <summary>
        /// Get final result and winner
        /// </summary>
        public ChallengeResult DetermineWinner()
        {
            var userScore = _userSimulation.ScenarioService.GetActiveScenario()?.CurrentScore ?? 0;
            var aiScore = _aiSimulation.ScenarioService.GetActiveScenario()?.CurrentScore ?? 0;

            string winner;
            float margin;

            if (userScore > aiScore)
            {
                winner = "Human";
                margin = userScore - aiScore;
            }
            else if (aiScore > userScore)
            {
                winner = "AI";
                margin = aiScore - userScore;
            }
            else
            {
                winner = "Tie";
                margin = 0;
            }

            return new ChallengeResult
            {
                ChallengeId = ChallengeId,
                Winner = winner,
                UserScore = userScore,
                AiScore = aiScore,
                Margin = margin,
                Duration = RealTimeSeconds,
                DurationSeconds = SimulationTimeSeconds
            };
        }

        /// <summary>
        /// Get command history for analysis/replay
        /// </summary>
        public List<ChallengeCommand> GetCommandHistory(bool forUser)
        {
            return forUser ? UserCommandHistory : AiCommandHistory;
        }

        /// <summary>
        /// Get both simulation states for rendering
        /// </summary>
        public (SimulationEngine User, SimulationEngine Ai) GetSimulationStates()
        {
            return (_userSimulation, _aiSimulation);
        }

        // Helper methods

        private Scenario CreateScenarioInstance(string scenarioId, Difficulty difficulty)
        {
            // TODO: Load scenario from repository with specified difficulty
            // For now, create a basic scenario
            var scenario = new Scenario
            {
                Metadata = new ScenarioMetadata
                {
                    Id = scenarioId,
                    Name = $"Challenge: {scenarioId}",
                    Difficulty = (ScenarioDifficulty)difficulty
                },
                Configuration = new ScenarioConfiguration()
            };

            return scenario;
        }

        private AircraftModel CreateTestAircraft()
        {
            return new AircraftModel
            {
                Callsign = "TEST123",
                AircraftType = "B738",
                IsArrival = true,
                PositionNm = new Vector2(10, 5), // 10nm from airport
                HeadingRadians = 3.14159f, // South
                SpeedKnots = 150,
                AltitudeFt = 3000,
                MinSpeedKnots = 100,
                MaxSpeedKnots = 250,
                MaxTurnRateRadPerSec = 0.1f
            };
        }

        private bool ShouldRequestAiCommand()
        {
            // Request AI command every 2 simulation seconds
            var lastPredictionTime = AIPredictions.LastOrDefault()?.Timestamp ?? 0;
            return (SimulationTimeSeconds - lastPredictionTime) >= 2.0f;
        }

        private GameObservation PrepareObservation(AircraftModel targetAircraft, SimulationEngine simulation)
        {
            return new GameObservation
            {
                AircraftAltitudeFt = targetAircraft.AltitudeFt,
                AircraftSpeedKts = targetAircraft.SpeedKnots,
                AircraftHeadingDeg = targetAircraft.HeadingRadians * 180f / MathF.PI,
                TargetAltitudeFt = targetAircraft.TargetAltitudeFt ?? targetAircraft.AltitudeFt,
                TargetSpeedKts = targetAircraft.TargetSpeedKnots ?? targetAircraft.SpeedKnots,
                TargetHeadingDeg = (targetAircraft.TargetHeadingRadians ?? targetAircraft.HeadingRadians) * 180f / MathF.PI,
                DistanceToAirportNm = CalculateDistanceToAirport(targetAircraft),
                AltitudeToRunwayFt = targetAircraft.AltitudeFt,
                WindSpeedKts = simulation.WeatherService.GetWeather("KJFK")?.WindLayers.FirstOrDefault()?.SpeedKnots ?? 0,
                WindDirectionDeg = simulation.WeatherService.GetWeather("KJFK")?.WindLayers.FirstOrDefault()?.DirectionDegrees ?? 0,
                SeparationFromOtherAircraftNm = FindClosestAircraft(targetAircraft, simulation),
                NumAircraftInApproach = CountApproachingAircraft(simulation)
            };
        }

        private string ConvertActionToCommand(MLAction action, AircraftModel aircraft, GameObservation observation)
        {
            var commands = new List<string>();

            // Generate heading command
            float headingDiff = Math.Abs(action.HeadingDeg - observation.AircraftHeadingDeg);
            if (headingDiff > 2)
            {
                if (action.HeadingDeg > observation.AircraftHeadingDeg)
                    commands.Add($"turn right heading {action.HeadingDeg:F0}");
                else
                    commands.Add($"turn left heading {action.HeadingDeg:F0}");
            }

            // Generate altitude command
            float altitudeDiff = Math.Abs(action.AltitudeFt - observation.AircraftAltitudeFt);
            if (altitudeDiff > 100)
            {
                if (action.AltitudeFt > observation.AircraftAltitudeFt)
                    commands.Add($"climb to {action.AltitudeFt:F0}");
                else
                    commands.Add($"descend to {action.AltitudeFt:F0}");
            }

            // Generate speed command
            float speedDiff = Math.Abs(action.SpeedKts - observation.AircraftSpeedKts);
            if (speedDiff > 5)
            {
                if (action.SpeedKts > observation.AircraftSpeedKts)
                    commands.Add($"increase speed to {action.SpeedKts:F0} knots");
                else
                    commands.Add($"reduce speed to {action.SpeedKts:F0} knots");
            }

            if (commands.Count == 0)
                commands.Add("maintain current state");

            return string.Join(" and ", commands);
        }

        private float CalculateDistanceToAirport(AircraftModel aircraft)
        {
            // TODO: Implement distance calculation to nearest airport
            return 10.0f; // Placeholder
        }

        private float FindClosestAircraft(AircraftModel targetAircraft, SimulationEngine simulation)
        {
            float minDistance = float.MaxValue;
            foreach (var other in simulation.Aircraft)
            {
                if (other != targetAircraft)
                {
                    float distance = Math.Abs(targetAircraft.PositionNm.X - other.PositionNm.X) +
                                   Math.Abs(targetAircraft.PositionNm.Y - other.PositionNm.Y);
                    minDistance = Math.Min(minDistance, distance);
                }
            }
            return minDistance == float.MaxValue ? 999 : minDistance;
        }

        private int CountApproachingAircraft(SimulationEngine simulation)
        {
            return simulation.Aircraft.Count(a => a.AltitudeFt < 5000); // Simplified
        }

        /// <summary>
        /// Apply a text command to an aircraft
        /// </summary>
        private void ApplyCommandToAircraft(AircraftModel? aircraft, string commandText)
        {
            if (aircraft == null) return;

            // Simple command parsing - in a real implementation this would be more sophisticated
            var command = commandText.ToLower().Trim();

            if (command.Contains("turn") && command.Contains("left"))
            {
                aircraft.ApplyAtcClearance(-0.1f, 0, 0); // Turn left
            }
            else if (command.Contains("turn") && command.Contains("right"))
            {
                aircraft.ApplyAtcClearance(0.1f, 0, 0); // Turn right
            }
            else if (command.Contains("climb"))
            {
                aircraft.ApplyAtcClearance(0, 0, 1000); // Climb
            }
            else if (command.Contains("descend"))
            {
                aircraft.ApplyAtcClearance(0, 0, -1000); // Descend
            }
            else if (command.Contains("speed up") || command.Contains("faster"))
            {
                aircraft.ApplyAtcClearance(0, 10, 0); // Speed up
            }
            else if (command.Contains("slow down") || command.Contains("slower"))
            {
                aircraft.ApplyAtcClearance(0, -10, 0); // Slow down
            }
            // Default: maintain current state
        }
    }

    // ============ Supporting Classes ============

    public enum ChallengeState
    {
        NotStarted,
        Ready,
        Running,
        Paused,
        Completed,
        Failed
    }

    public class ChallengeCommand
    {
        public string CommandText { get; set; }
        public string TargetCallsign { get; set; }
        public float Timestamp { get; set; }
        public bool IssuedByAI { get; set; }
        public float Confidence { get; set; } = 1.0f; // AI confidence or 1.0 for human
    }

    public class AIPrediction
    {
        public float Timestamp { get; set; }
        public string Callsign { get; set; }
        public string Command { get; set; }
        public float Confidence { get; set; }
    }

    public class AiCommandResult
    {
        public string Command { get; set; }
        public MLAction Action { get; set; }
        public float Confidence { get; set; }
    }

    public class ChallengeComparison
    {
        public SessionScore UserScore { get; set; }
        public SessionScore AiScore { get; set; }
        public int UserAircraftCount { get; set; }
        public int AiAircraftCount { get; set; }
        public int UserCommandCount { get; set; }
        public int AiCommandCount { get; set; }
        public float SimulationTimeSeconds { get; set; }
    }

    public class ChallengeResult
    {
        public string ChallengeId { get; set; }
        public string Winner { get; set; } // "Human", "AI", or "Tie"
        public float UserScore { get; set; }
        public float AiScore { get; set; }
        public float Margin { get; set; }
        public float Duration { get; set; }
        public float DurationSeconds { get; set; }
    }

    public class ChallengeStateChangedEventArgs : EventArgs
    {
        public ChallengeState NewState { get; }

        public ChallengeStateChangedEventArgs(ChallengeState newState)
        {
            NewState = newState;
        }
    }
}
