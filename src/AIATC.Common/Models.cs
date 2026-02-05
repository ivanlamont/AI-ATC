using System;

namespace AIATC.Common
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
        public long InferenceTimeMs { get; set; }
    }

    /// <summary>
    /// Difficulty levels for challenges
    /// </summary>
    public enum Difficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Expert = 4
    }

    /// <summary>
    /// Event args for separation violations
    /// </summary>
    public class SeparationViolationEventArgs : EventArgs
    {
        public string AircraftId1 { get; set; }
        public string AircraftId2 { get; set; }
        public float DistanceNm { get; set; }
        public float RequiredSeparationNm { get; set; }
        public DateTime Timestamp { get; set; }

        public SeparationViolationEventArgs(string aircraftId1, string aircraftId2, float distanceNm, float requiredSeparationNm)
        {
            AircraftId1 = aircraftId1;
            AircraftId2 = aircraftId2;
            DistanceNm = distanceNm;
            RequiredSeparationNm = requiredSeparationNm;
            Timestamp = DateTime.UtcNow;
        }
    }
}