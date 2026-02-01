"""
Tests for runway configuration management system.
"""

import pytest
import numpy as np
from runway_config import (
    RunwayOrientation,
    RunwayStatus,
    WindConditions,
    RunwayConfig,
    RunwayConfigurationManager,
)
from airport import Airport


class TestWindConditions:
    """Test wind condition calculations."""

    def test_wind_creation(self):
        """Test creating wind conditions."""
        wind = WindConditions(15.0, 270.0, 5.0)

        assert wind.wind_speed_kts == 15.0
        assert wind.wind_direction_deg == 270.0
        assert wind.wind_gust_kts == 5.0

    def test_headwind_calculation_direct(self):
        """Test headwind calculation for direct headwind."""
        wind = WindConditions(10.0, 90.0)  # Wind from east

        # RWY 09 heading east - direct headwind
        headwind = wind.get_headwind_component(90.0)

        assert headwind > 9.0  # Should be close to 10 knots

    def test_tailwind_calculation_direct(self):
        """Test tailwind calculation for direct tailwind."""
        wind = WindConditions(10.0, 270.0)  # Wind from west

        # RWY 09 heading east - direct tailwind
        tailwind = wind.get_headwind_component(90.0)

        assert tailwind < -9.0  # Should be close to -10 knots

    def test_crosswind_calculation(self):
        """Test crosswind calculation."""
        wind = WindConditions(10.0, 180.0)  # Wind from south

        # RWY 27 heading west - full crosswind
        crosswind = wind.get_crosswind_component(270.0)

        assert abs(abs(crosswind) - 10.0) < 0.5  # Should be ~10 knots

    def test_no_wind(self):
        """Test zero wind conditions."""
        wind = WindConditions(0.0, 0.0)

        assert wind.get_headwind_component(90.0) == 0.0
        assert wind.get_crosswind_component(90.0) == 0.0


class TestRunwayConfig:
    """Test runway configuration."""

    def test_runway_creation(self):
        """Test creating runway configuration."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
        )

        assert runway.runway_id == "RWY 27"
        assert runway.runway_heading_deg == 270.0
        assert runway.status == RunwayStatus.ACTIVE

    def test_runway_operational_check(self):
        """Test runway operational status."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
        )

        assert runway.is_operational()

        runway.status = RunwayStatus.CLOSED
        assert not runway.is_operational()

    def test_runway_can_accept_aircraft_ideal(self):
        """Test runway acceptance with ideal conditions."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
        )

        wind = WindConditions(10.0, 270.0)  # Direct headwind

        can_accept, reason = runway.can_accept_aircraft(wind)

        assert can_accept is True
        assert reason is None

    def test_runway_excessive_crosswind(self):
        """Test runway rejection for excessive crosswind."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
            max_crosswind_kts=10.0,
        )

        wind = WindConditions(20.0, 180.0)  # Strong crosswind

        can_accept, reason = runway.can_accept_aircraft(wind)

        assert can_accept is False
        assert "Crosswind" in reason

    def test_runway_excessive_tailwind(self):
        """Test runway rejection for excessive tailwind."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
            max_tailwind_kts=5.0,
        )

        wind = WindConditions(15.0, 90.0)  # Strong tailwind

        can_accept, reason = runway.can_accept_aircraft(wind)

        assert can_accept is False
        assert "Tailwind" in reason

    def test_runway_suitability_score(self):
        """Test runway suitability scoring."""
        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
        )

        # Ideal conditions
        wind_ideal = WindConditions(10.0, 270.0)  # Direct headwind
        score_ideal = runway.get_suitability_score(wind_ideal)

        # Poor conditions
        wind_poor = WindConditions(15.0, 180.0)  # Crosswind
        score_poor = runway.get_suitability_score(wind_poor)

        assert score_ideal > score_poor


class TestRunwayConfigurationManager:
    """Test runway configuration manager."""

    def test_manager_creation(self):
        """Test creating runway manager."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        assert len(manager.runways) == 0
        assert manager.active_runway is None

    def test_add_runway(self):
        """Test adding runways."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        runway = RunwayConfig(
            runway_id="RWY 27",
            runway_heading_deg=270.0,
            length_ft=10000.0,
            width_ft=150.0,
        )

        manager.add_runway(runway)

        assert "RWY 27" in manager.runways
        assert manager.active_runway == "RWY 27"

    def test_multiple_runways(self):
        """Test managing multiple runways."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27L", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 27R", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09L", 90.0, 8000.0, 150.0))

        assert len(manager.runways) == 3

    def test_wind_update(self):
        """Test updating wind conditions."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.update_wind_conditions(15.0, 270.0, 5.0)

        assert manager.wind_conditions.wind_speed_kts == 15.0
        assert manager.wind_conditions.wind_direction_deg == 270.0

    def test_best_runway_selection(self):
        """Test selecting best runway for wind."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        # Westerly wind - good for RWY 27
        manager.update_wind_conditions(10.0, 270.0)
        best = manager.get_best_runway()

        assert best == "RWY 27"

        # Easterly wind - good for RWY 09
        manager.update_wind_conditions(10.0, 90.0)
        best = manager.get_best_runway()

        assert best == "RWY 09"

    def test_evaluate_configuration_change(self):
        """Test evaluating need for configuration change."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        # Start with westerly wind
        manager.update_wind_conditions(10.0, 270.0)
        manager.active_runway = "RWY 27"

        # Change to easterly wind
        manager.update_wind_conditions(10.0, 90.0)
        should_change, new_runway, reason = manager.evaluate_configuration_change(
            current_time=1000.0
        )

        assert should_change is True
        assert new_runway == "RWY 09"

    def test_minimum_time_between_changes(self):
        """Test minimum time between runway changes."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        # Make first change
        manager.update_wind_conditions(10.0, 90.0)
        manager.change_runway_configuration("RWY 09", 1000.0)

        # Try to change again immediately
        manager.update_wind_conditions(10.0, 270.0)
        should_change, _, _ = manager.evaluate_configuration_change(1050.0)

        assert should_change is False  # Too soon

    def test_change_runway_configuration(self):
        """Test changing runway configuration."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        success, message = manager.change_runway_configuration("RWY 09", 1000.0)

        assert success is True
        assert manager.active_runway == "RWY 09"
        assert "RWY 27" in message and "RWY 09" in message

    def test_close_runway(self):
        """Test closing runway."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        manager.active_runway = "RWY 27"
        manager.close_runway("RWY 27")

        assert manager.runways["RWY 27"].status == RunwayStatus.CLOSED

    def test_reopen_runway(self):
        """Test reopening closed runway."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        runway = RunwayConfig("RWY 27", 270.0, 10000.0, 150.0)
        manager.add_runway(runway)

        manager.close_runway("RWY 27")
        assert not manager.runways["RWY 27"].is_operational()

        manager.reopen_runway("RWY 27")
        assert manager.runways["RWY 27"].is_operational()

    def test_configuration_history(self):
        """Test tracking configuration changes."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.add_runway(RunwayConfig("RWY 09", 90.0, 8000.0, 150.0))

        manager.change_runway_configuration("RWY 09", 1000.0)
        manager.change_runway_configuration("RWY 27", 2000.0)

        assert len(manager.configuration_history) == 2
        assert manager.configuration_history[0] == (1000.0, "RWY 27", "RWY 09")
        assert manager.configuration_history[1] == (2000.0, "RWY 09", "RWY 27")

    def test_get_summary(self):
        """Test getting configuration summary."""
        airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
        manager = RunwayConfigurationManager(airport)

        manager.add_runway(RunwayConfig("RWY 27", 270.0, 10000.0, 150.0))
        manager.update_wind_conditions(10.0, 270.0)

        summary = manager.get_summary()

        assert isinstance(summary, str)
        assert "RWY 27" in summary
        assert "RUNWAY CONFIGURATION" in summary


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
