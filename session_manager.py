"""
Session management system for AI-ATC.
Handles session recording, saving, loading, and replaying ATC scenarios.
"""

import json
import numpy as np
from dataclasses import dataclass, asdict, field
from typing import List, Dict, Optional, Any, Tuple
from enum import Enum
from datetime import datetime
import gzip
import os


class EventType(Enum):
    """Types of events that can occur during a session."""
    AIRCRAFT_SPAWN = "aircraft_spawn"
    AIRCRAFT_LANDED = "aircraft_landed"
    AIRCRAFT_CRASHED = "aircraft_crashed"
    ATC_CLEARANCE = "atc_clearance"
    RUNWAY_CHANGE = "runway_change"
    WEATHER_UPDATE = "weather_update"
    SEPARATION_VIOLATION = "separation_violation"
    SIMULATION_START = "simulation_start"
    SIMULATION_END = "simulation_end"
    EPISODE_REWARD = "episode_reward"


@dataclass
class AircraftSnapshot:
    """Snapshot of aircraft state at a point in time."""
    plane_id: int
    position_nm: Tuple[float, float]
    heading_rad: float
    speed_kts: float
    altitude_ft: float
    vert_speed: float
    target_heading: float
    target_speed: float
    target_altitude: float
    landed: bool


@dataclass
class SessionEvent:
    """A single event that occurred during a session."""
    timestamp: float
    event_type: EventType
    description: str
    data: Dict[str, Any] = field(default_factory=dict)

    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for serialization."""
        return {
            'timestamp': self.timestamp,
            'event_type': self.event_type.value,
            'description': self.description,
            'data': self.data,
        }

    @staticmethod
    def from_dict(data: Dict[str, Any]) -> 'SessionEvent':
        """Create from dictionary."""
        return SessionEvent(
            timestamp=data['timestamp'],
            event_type=EventType(data['event_type']),
            description=data['description'],
            data=data.get('data', {}),
        )


@dataclass
class SessionCheckpoint:
    """Checkpoint of session state at a specific time."""
    timestamp: float
    step: int
    aircraft_snapshots: List[AircraftSnapshot]
    wind_speed: float
    wind_direction: float
    active_runway: str
    total_reward: float


@dataclass
class SessionMetadata:
    """Metadata about a recorded session."""
    session_id: str
    start_time: str  # ISO format datetime
    end_time: Optional[str] = None
    duration_seconds: float = 0.0
    episode_count: int = 0
    total_reward: float = 0.0
    airports_used: List[str] = field(default_factory=list)
    aircraft_count: int = 0
    landings_successful: int = 0
    crashes: int = 0
    separation_violations: int = 0
    model_version: str = ""
    notes: str = ""


class SessionRecorder:
    """Records all events and state during a simulation."""

    def __init__(self, session_id: str):
        self.session_id = session_id
        self.start_time = datetime.utcnow()
        self.events: List[SessionEvent] = []
        self.checkpoints: List[SessionCheckpoint] = []
        self.current_step = 0
        self.total_reward = 0.0

        # Track statistics
        self.aircraft_spawned = set()
        self.successful_landings = 0
        self.crashes = 0
        self.separation_violations = 0

    def record_event(
        self,
        timestamp: float,
        event_type: EventType,
        description: str,
        data: Optional[Dict[str, Any]] = None,
    ) -> None:
        """Record an event during simulation."""
        event = SessionEvent(
            timestamp=timestamp,
            event_type=event_type,
            description=description,
            data=data or {},
        )
        self.events.append(event)

    def record_aircraft_spawn(self, plane_id: int, timestamp: float, is_vfr: bool = False) -> None:
        """Record aircraft spawn."""
        self.aircraft_spawned.add(plane_id)
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.AIRCRAFT_SPAWN,
            description=f"Aircraft {plane_id} spawned ({'VFR' if is_vfr else 'IFR'})",
            data={'plane_id': plane_id, 'is_vfr': is_vfr},
        )

    def record_landing(self, plane_id: int, timestamp: float) -> None:
        """Record successful aircraft landing."""
        self.successful_landings += 1
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.AIRCRAFT_LANDED,
            description=f"Aircraft {plane_id} landed successfully",
            data={'plane_id': plane_id},
        )

    def record_crash(self, plane_id: int, timestamp: float, reason: str = "") -> None:
        """Record aircraft crash."""
        self.crashes += 1
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.AIRCRAFT_CRASHED,
            description=f"Aircraft {plane_id} crashed" + (f": {reason}" if reason else ""),
            data={'plane_id': plane_id, 'reason': reason},
        )

    def record_runway_change(
        self,
        timestamp: float,
        from_runway: str,
        to_runway: str,
    ) -> None:
        """Record runway configuration change."""
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.RUNWAY_CHANGE,
            description=f"Runway changed from {from_runway} to {to_runway}",
            data={'from_runway': from_runway, 'to_runway': to_runway},
        )

    def record_atc_clearance(
        self,
        timestamp: float,
        plane_id: int,
        clearance_type: str,
        details: str,
    ) -> None:
        """Record ATC clearance."""
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.ATC_CLEARANCE,
            description=f"Clearance to {plane_id}: {details}",
            data={
                'plane_id': plane_id,
                'clearance_type': clearance_type,
                'details': details,
            },
        )

    def record_separation_violation(
        self,
        timestamp: float,
        plane_id_1: int,
        plane_id_2: int,
        distance_nm: float,
    ) -> None:
        """Record separation violation."""
        self.separation_violations += 1
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.SEPARATION_VIOLATION,
            description=f"Separation violation: Aircraft {plane_id_1} and {plane_id_2} at {distance_nm:.1f} NM",
            data={
                'plane_id_1': plane_id_1,
                'plane_id_2': plane_id_2,
                'distance_nm': distance_nm,
            },
        )

    def record_weather_update(self, timestamp: float, wind_speed: float, wind_direction: float) -> None:
        """Record weather update."""
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.WEATHER_UPDATE,
            description=f"Wind: {wind_speed:.1f} kts from {wind_direction:.0f}°",
            data={'wind_speed': wind_speed, 'wind_direction': wind_direction},
        )

    def record_episode_reward(self, timestamp: float, reward: float) -> None:
        """Record episode reward."""
        self.total_reward += reward
        self.record_event(
            timestamp=timestamp,
            event_type=EventType.EPISODE_REWARD,
            description=f"Episode reward: {reward:.2f}",
            data={'reward': reward, 'total_reward': self.total_reward},
        )

    def create_checkpoint(
        self,
        timestamp: float,
        aircraft_snapshots: List[AircraftSnapshot],
        wind_speed: float,
        wind_direction: float,
        active_runway: str,
        total_reward: float = 0.0,
    ) -> None:
        """Create a checkpoint of current session state."""
        checkpoint = SessionCheckpoint(
            timestamp=timestamp,
            step=self.current_step,
            aircraft_snapshots=aircraft_snapshots,
            wind_speed=wind_speed,
            wind_direction=wind_direction,
            active_runway=active_runway,
            total_reward=total_reward,
        )
        self.checkpoints.append(checkpoint)
        self.current_step += 1

    def get_session_metadata(self) -> SessionMetadata:
        """Generate session metadata."""
        end_time = datetime.utcnow()
        duration = (end_time - self.start_time).total_seconds()

        return SessionMetadata(
            session_id=self.session_id,
            start_time=self.start_time.isoformat(),
            end_time=end_time.isoformat(),
            duration_seconds=duration,
            episode_count=len(self.checkpoints),
            total_reward=self.total_reward,
            aircraft_count=len(self.aircraft_spawned),
            landings_successful=self.successful_landings,
            crashes=self.crashes,
            separation_violations=self.separation_violations,
        )


class SessionSerializer:
    """Serializes and deserializes sessions to/from disk."""

    @staticmethod
    def save_session(
        recorder: SessionRecorder,
        filepath: str,
        compress: bool = True,
    ) -> bool:
        """
        Save a recorded session to disk.

        Args:
            recorder: SessionRecorder with recorded data
            filepath: Path to save to
            compress: Whether to gzip compress the file

        Returns:
            Success status
        """
        try:
            # Prepare data
            metadata = recorder.get_session_metadata()

            # Convert events
            events_data = [event.to_dict() for event in recorder.events]

            # Convert checkpoints
            checkpoints_data = []
            for cp in recorder.checkpoints:
                checkpoints_data.append({
                    'timestamp': cp.timestamp,
                    'step': cp.step,
                    'aircraft_snapshots': [
                        {
                            'plane_id': snap.plane_id,
                            'position_nm': snap.position_nm,
                            'heading_rad': snap.heading_rad,
                            'speed_kts': snap.speed_kts,
                            'altitude_ft': snap.altitude_ft,
                            'vert_speed': snap.vert_speed,
                            'target_heading': snap.target_heading,
                            'target_speed': snap.target_speed,
                            'target_altitude': snap.target_altitude,
                            'landed': snap.landed,
                        }
                        for snap in cp.aircraft_snapshots
                    ],
                    'wind_speed': cp.wind_speed,
                    'wind_direction': cp.wind_direction,
                    'active_runway': cp.active_runway,
                    'total_reward': cp.total_reward,
                })

            session_data = {
                'metadata': asdict(metadata),
                'events': events_data,
                'checkpoints': checkpoints_data,
            }

            # Serialize to JSON
            json_data = json.dumps(session_data, indent=2)

            # Write to file
            if compress:
                filepath = filepath if filepath.endswith('.gz') else filepath + '.gz'
                with gzip.open(filepath, 'wt', encoding='utf-8') as f:
                    f.write(json_data)
            else:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(json_data)

            return True

        except Exception as e:
            print(f"Error saving session: {e}")
            return False

    @staticmethod
    def load_session(filepath: str) -> Optional[Tuple[SessionMetadata, List[SessionEvent], List[SessionCheckpoint]]]:
        """
        Load a session from disk.

        Args:
            filepath: Path to load from

        Returns:
            Tuple of (metadata, events, checkpoints) or None if error
        """
        try:
            # Read from file
            if filepath.endswith('.gz') or os.path.exists(filepath + '.gz'):
                if not filepath.endswith('.gz'):
                    filepath = filepath + '.gz'
                with gzip.open(filepath, 'rt', encoding='utf-8') as f:
                    json_data = f.read()
            else:
                with open(filepath, 'r', encoding='utf-8') as f:
                    json_data = f.read()

            # Parse JSON
            session_data = json.loads(json_data)

            # Reconstruct metadata
            metadata_dict = session_data['metadata']
            metadata = SessionMetadata(**metadata_dict)

            # Reconstruct events
            events = [SessionEvent.from_dict(e) for e in session_data['events']]

            # Reconstruct checkpoints
            checkpoints = []
            for cp_data in session_data['checkpoints']:
                snapshots = [
                    AircraftSnapshot(
                        plane_id=snap['plane_id'],
                        position_nm=tuple(snap['position_nm']),
                        heading_rad=snap['heading_rad'],
                        speed_kts=snap['speed_kts'],
                        altitude_ft=snap['altitude_ft'],
                        vert_speed=snap['vert_speed'],
                        target_heading=snap['target_heading'],
                        target_speed=snap['target_speed'],
                        target_altitude=snap['target_altitude'],
                        landed=snap['landed'],
                    )
                    for snap in cp_data['aircraft_snapshots']
                ]

                checkpoint = SessionCheckpoint(
                    timestamp=cp_data['timestamp'],
                    step=cp_data['step'],
                    aircraft_snapshots=snapshots,
                    wind_speed=cp_data['wind_speed'],
                    wind_direction=cp_data['wind_direction'],
                    active_runway=cp_data['active_runway'],
                    total_reward=cp_data['total_reward'],
                )
                checkpoints.append(checkpoint)

            return metadata, events, checkpoints

        except Exception as e:
            print(f"Error loading session: {e}")
            return None


class SessionReplayer:
    """Replays recorded sessions."""

    def __init__(
        self,
        metadata: SessionMetadata,
        events: List[SessionEvent],
        checkpoints: List[SessionCheckpoint],
    ):
        self.metadata = metadata
        self.events = events
        self.checkpoints = checkpoints
        self.current_checkpoint_idx = 0

    def get_events_at_timestamp(self, timestamp: float) -> List[SessionEvent]:
        """Get all events at a specific timestamp."""
        return [e for e in self.events if e.timestamp == timestamp]

    def get_events_in_range(self, start_time: float, end_time: float) -> List[SessionEvent]:
        """Get all events within a time range."""
        return [e for e in self.events if start_time <= e.timestamp <= end_time]

    def get_checkpoint_at_time(self, timestamp: float) -> Optional[SessionCheckpoint]:
        """Get the checkpoint closest to a specific timestamp."""
        best_cp = None
        best_diff = float('inf')

        for cp in self.checkpoints:
            diff = abs(cp.timestamp - timestamp)
            if diff < best_diff:
                best_diff = diff
                best_cp = cp

        return best_cp

    def get_summary(self) -> str:
        """Get summary of recorded session."""
        summary = f"\n{'='*60}\n"
        summary += f"SESSION SUMMARY\n"
        summary += f"{'='*60}\n"
        summary += f"Session ID: {self.metadata.session_id}\n"
        summary += f"Start Time: {self.metadata.start_time}\n"
        summary += f"Duration: {self.metadata.duration_seconds:.0f} seconds\n"
        summary += f"Episodes: {self.metadata.episode_count}\n"
        summary += f"Total Reward: {self.metadata.total_reward:.2f}\n"
        summary += f"\nOperations:\n"
        summary += f"  Aircraft Spawned: {self.metadata.aircraft_count}\n"
        summary += f"  Successful Landings: {self.metadata.landings_successful}\n"
        summary += f"  Crashes: {self.metadata.crashes}\n"
        summary += f"  Separation Violations: {self.metadata.separation_violations}\n"
        summary += f"{'='*60}\n"
        return summary


if __name__ == "__main__":
    # Test session management
    print("Testing Session Management System")
    print("=" * 60)

    # Create and populate a recorder
    recorder = SessionRecorder("test-session-001")

    recorder.record_event(0.0, EventType.SIMULATION_START, "Simulation started")
    recorder.record_aircraft_spawn(1, 0.5, is_vfr=False)
    recorder.record_aircraft_spawn(2, 1.0, is_vfr=True)
    recorder.record_weather_update(2.0, 15.0, 270.0)
    recorder.record_landing(1, 300.0)
    recorder.record_episode_reward(301.0, 85.5)

    print(f"Recorded {len(recorder.events)} events")
    print(f"Created {len(recorder.checkpoints)} checkpoints")

    # Save session
    filepath = "sessions/test_session.json"
    os.makedirs("sessions", exist_ok=True)

    if SessionSerializer.save_session(recorder, filepath):
        print(f"✓ Session saved to {filepath}")

        # Load session
        result = SessionSerializer.load_session(filepath)
        if result:
            metadata, events, checkpoints = result
            print(f"✓ Session loaded: {metadata.session_id}")
            print(f"  Events: {len(events)}")
            print(f"  Checkpoints: {len(checkpoints)}")

            # Create replayer
            replayer = SessionReplayer(metadata, events, checkpoints)
            print(replayer.get_summary())
