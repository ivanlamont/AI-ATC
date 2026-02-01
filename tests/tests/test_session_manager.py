"""
Tests for session management system.
"""

import pytest
import os
import tempfile
import gzip
import json
from session_manager import (
    EventType,
    AircraftSnapshot,
    SessionEvent,
    SessionCheckpoint,
    SessionMetadata,
    SessionRecorder,
    SessionSerializer,
    SessionReplayer,
)


class TestEventType:
    """Test event types."""

    def test_event_types_exist(self):
        """Test that all event types are defined."""
        assert EventType.AIRCRAFT_SPAWN
        assert EventType.AIRCRAFT_LANDED
        assert EventType.AIRCRAFT_CRASHED
        assert EventType.ATC_CLEARANCE
        assert EventType.RUNWAY_CHANGE
        assert EventType.WEATHER_UPDATE
        assert EventType.SEPARATION_VIOLATION
        assert EventType.SIMULATION_START
        assert EventType.SIMULATION_END
        assert EventType.EPISODE_REWARD


class TestSessionEvent:
    """Test session events."""

    def test_event_creation(self):
        """Test creating an event."""
        event = SessionEvent(
            timestamp=10.0,
            event_type=EventType.AIRCRAFT_SPAWN,
            description="Aircraft 1 spawned",
            data={'plane_id': 1},
        )

        assert event.timestamp == 10.0
        assert event.event_type == EventType.AIRCRAFT_SPAWN

    def test_event_to_dict(self):
        """Test event serialization."""
        event = SessionEvent(
            timestamp=10.0,
            event_type=EventType.AIRCRAFT_SPAWN,
            description="Aircraft 1 spawned",
            data={'plane_id': 1},
        )

        event_dict = event.to_dict()

        assert event_dict['timestamp'] == 10.0
        assert event_dict['event_type'] == 'aircraft_spawn'
        assert event_dict['data']['plane_id'] == 1

    def test_event_from_dict(self):
        """Test event deserialization."""
        event_dict = {
            'timestamp': 10.0,
            'event_type': 'aircraft_spawn',
            'description': 'Aircraft 1 spawned',
            'data': {'plane_id': 1},
        }

        event = SessionEvent.from_dict(event_dict)

        assert event.timestamp == 10.0
        assert event.event_type == EventType.AIRCRAFT_SPAWN


class TestAircraftSnapshot:
    """Test aircraft snapshots."""

    def test_snapshot_creation(self):
        """Test creating snapshot."""
        snapshot = AircraftSnapshot(
            plane_id=1,
            position_nm=(10.0, 5.0),
            heading_rad=1.57,
            speed_kts=150.0,
            altitude_ft=3000.0,
            vert_speed=0.0,
            target_heading=1.57,
            target_speed=150.0,
            target_altitude=3000.0,
            landed=False,
        )

        assert snapshot.plane_id == 1
        assert snapshot.speed_kts == 150.0


class TestSessionRecorder:
    """Test session recording."""

    def test_recorder_creation(self):
        """Test creating recorder."""
        recorder = SessionRecorder("test-session")

        assert recorder.session_id == "test-session"
        assert len(recorder.events) == 0
        assert recorder.total_reward == 0.0

    def test_record_event(self):
        """Test recording event."""
        recorder = SessionRecorder("test-session")

        recorder.record_event(
            timestamp=10.0,
            event_type=EventType.AIRCRAFT_SPAWN,
            description="Aircraft 1 spawned",
        )

        assert len(recorder.events) == 1
        assert recorder.events[0].event_type == EventType.AIRCRAFT_SPAWN

    def test_record_aircraft_spawn(self):
        """Test recording aircraft spawn."""
        recorder = SessionRecorder("test-session")

        recorder.record_aircraft_spawn(plane_id=1, timestamp=5.0)

        assert len(recorder.events) == 1
        assert 1 in recorder.aircraft_spawned

    def test_record_landing(self):
        """Test recording landing."""
        recorder = SessionRecorder("test-session")

        recorder.record_landing(plane_id=1, timestamp=100.0)

        assert recorder.successful_landings == 1
        assert len(recorder.events) == 1

    def test_record_crash(self):
        """Test recording crash."""
        recorder = SessionRecorder("test-session")

        recorder.record_crash(plane_id=1, timestamp=100.0, reason="Collision")

        assert recorder.crashes == 1

    def test_record_separation_violation(self):
        """Test recording separation violation."""
        recorder = SessionRecorder("test-session")

        recorder.record_separation_violation(
            timestamp=50.0,
            plane_id_1=1,
            plane_id_2=2,
            distance_nm=1.5,
        )

        assert recorder.separation_violations == 1

    def test_record_episode_reward(self):
        """Test recording episode reward."""
        recorder = SessionRecorder("test-session")

        recorder.record_episode_reward(timestamp=100.0, reward=75.5)
        recorder.record_episode_reward(timestamp=200.0, reward=85.0)

        assert recorder.total_reward == pytest.approx(160.5)

    def test_create_checkpoint(self):
        """Test creating checkpoint."""
        recorder = SessionRecorder("test-session")

        snapshot = AircraftSnapshot(
            plane_id=1,
            position_nm=(10.0, 5.0),
            heading_rad=1.57,
            speed_kts=150.0,
            altitude_ft=3000.0,
            vert_speed=0.0,
            target_heading=1.57,
            target_speed=150.0,
            target_altitude=3000.0,
            landed=False,
        )

        recorder.create_checkpoint(
            timestamp=10.0,
            aircraft_snapshots=[snapshot],
            wind_speed=10.0,
            wind_direction=270.0,
            active_runway="RWY 27",
        )

        assert len(recorder.checkpoints) == 1
        assert recorder.checkpoints[0].step == 0

    def test_get_session_metadata(self):
        """Test getting session metadata."""
        recorder = SessionRecorder("test-session")

        recorder.record_aircraft_spawn(1, 0.0)
        recorder.record_landing(1, 100.0)
        recorder.record_episode_reward(101.0, 50.0)

        metadata = recorder.get_session_metadata()

        assert metadata.session_id == "test-session"
        assert metadata.aircraft_count == 1
        assert metadata.landings_successful == 1
        assert metadata.total_reward == 50.0


class TestSessionSerializer:
    """Test session serialization."""

    def test_save_and_load_session(self):
        """Test saving and loading session."""
        recorder = SessionRecorder("test-session")

        recorder.record_aircraft_spawn(1, 0.5)
        recorder.record_aircraft_spawn(2, 1.0)
        recorder.record_landing(1, 100.0)
        recorder.record_episode_reward(101.0, 75.5)

        with tempfile.TemporaryDirectory() as tmpdir:
            filepath = os.path.join(tmpdir, "test_session.json")

            # Save
            success = SessionSerializer.save_session(recorder, filepath, compress=False)
            assert success

            # Load
            result = SessionSerializer.load_session(filepath)
            assert result is not None

            metadata, events, checkpoints = result
            assert metadata.session_id == "test-session"
            assert len(events) == 4
            assert metadata.landings_successful == 1
            assert metadata.total_reward == 75.5

    def test_save_and_load_compressed(self):
        """Test saving and loading compressed session."""
        recorder = SessionRecorder("test-session")

        recorder.record_aircraft_spawn(1, 0.5)
        recorder.record_landing(1, 100.0)

        with tempfile.TemporaryDirectory() as tmpdir:
            filepath = os.path.join(tmpdir, "test_session.json")

            # Save compressed
            success = SessionSerializer.save_session(recorder, filepath, compress=True)
            assert success

            # File should be compressed
            assert os.path.exists(filepath + ".gz")

            # Load
            result = SessionSerializer.load_session(filepath)
            assert result is not None

            metadata, events, checkpoints = result
            assert metadata.session_id == "test-session"

    def test_save_with_checkpoints(self):
        """Test saving session with checkpoints."""
        recorder = SessionRecorder("test-session")

        snapshot = AircraftSnapshot(
            plane_id=1,
            position_nm=(10.0, 5.0),
            heading_rad=1.57,
            speed_kts=150.0,
            altitude_ft=3000.0,
            vert_speed=0.0,
            target_heading=1.57,
            target_speed=150.0,
            target_altitude=3000.0,
            landed=False,
        )

        recorder.create_checkpoint(
            timestamp=10.0,
            aircraft_snapshots=[snapshot],
            wind_speed=10.0,
            wind_direction=270.0,
            active_runway="RWY 27",
        )

        with tempfile.TemporaryDirectory() as tmpdir:
            filepath = os.path.join(tmpdir, "test_session.json")

            SessionSerializer.save_session(recorder, filepath, compress=False)

            result = SessionSerializer.load_session(filepath)
            assert result is not None

            metadata, events, checkpoints = result
            assert len(checkpoints) == 1
            assert checkpoints[0].aircraft_snapshots[0].plane_id == 1


class TestSessionReplayer:
    """Test session replay."""

    def test_replayer_creation(self):
        """Test creating replayer."""
        metadata = SessionMetadata("test-session", "2024-01-01T00:00:00")
        replayer = SessionReplayer(metadata, [], [])

        assert replayer.metadata.session_id == "test-session"

    def test_get_events_at_timestamp(self):
        """Test getting events at specific timestamp."""
        metadata = SessionMetadata("test-session", "2024-01-01T00:00:00")
        events = [
            SessionEvent(10.0, EventType.AIRCRAFT_SPAWN, "Aircraft 1 spawned"),
            SessionEvent(10.0, EventType.AIRCRAFT_SPAWN, "Aircraft 2 spawned"),
            SessionEvent(20.0, EventType.AIRCRAFT_LANDED, "Aircraft 1 landed"),
        ]

        replayer = SessionReplayer(metadata, events, [])
        events_at_10 = replayer.get_events_at_timestamp(10.0)

        assert len(events_at_10) == 2

    def test_get_events_in_range(self):
        """Test getting events in time range."""
        metadata = SessionMetadata("test-session", "2024-01-01T00:00:00")
        events = [
            SessionEvent(10.0, EventType.AIRCRAFT_SPAWN, "Aircraft 1 spawned"),
            SessionEvent(15.0, EventType.WEATHER_UPDATE, "Wind updated"),
            SessionEvent(20.0, EventType.AIRCRAFT_LANDED, "Aircraft 1 landed"),
            SessionEvent(30.0, EventType.EPISODE_REWARD, "Episode complete"),
        ]

        replayer = SessionReplayer(metadata, events, [])
        range_events = replayer.get_events_in_range(10.0, 20.0)

        assert len(range_events) == 3

    def test_get_checkpoint_at_time(self):
        """Test getting checkpoint at time."""
        metadata = SessionMetadata("test-session", "2024-01-01T00:00:00")

        checkpoints = [
            SessionCheckpoint(10.0, 0, [], 10.0, 270.0, "RWY 27", 0.0),
            SessionCheckpoint(20.0, 1, [], 15.0, 270.0, "RWY 27", 50.0),
            SessionCheckpoint(30.0, 2, [], 12.0, 270.0, "RWY 27", 100.0),
        ]

        replayer = SessionReplayer(metadata, [], checkpoints)

        # Get checkpoint closest to 11.0
        cp = replayer.get_checkpoint_at_time(11.0)
        assert cp is not None
        assert cp.timestamp == 10.0

    def test_get_summary(self):
        """Test getting replay summary."""
        metadata = SessionMetadata(
            session_id="test-session",
            start_time="2024-01-01T00:00:00",
            duration_seconds=300.0,
            episode_count=1,
            total_reward=85.5,
            aircraft_count=3,
            landings_successful=2,
            crashes=1,
            separation_violations=0,
        )

        replayer = SessionReplayer(metadata, [], [])
        summary = replayer.get_summary()

        assert isinstance(summary, str)
        assert "test-session" in summary
        assert "85.5" in summary


class TestSessionIntegration:
    """Integration tests for session management."""

    def test_full_session_lifecycle(self):
        """Test complete session recording and replay."""
        # Record
        recorder = SessionRecorder("integration-test")

        for i in range(1, 4):
            recorder.record_aircraft_spawn(i, i * 0.5)
            recorder.record_landing(i, i * 100.0)
            recorder.record_episode_reward(i * 100.0 + 1, i * 50.0)

        # Save
        with tempfile.TemporaryDirectory() as tmpdir:
            filepath = os.path.join(tmpdir, "integration_test.json")
            SessionSerializer.save_session(recorder, filepath, compress=False)

            # Load
            result = SessionSerializer.load_session(filepath)
            assert result is not None

            metadata, events, checkpoints = result

            # Replay
            replayer = SessionReplayer(metadata, events, checkpoints)

            # Verify
            assert replayer.metadata.aircraft_count == 3
            assert replayer.metadata.landings_successful == 3
            assert len(replayer.events) == 9  # 3 spawns + 3 landings + 3 rewards

            summary = replayer.get_summary()
            assert "integration-test" in summary


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
