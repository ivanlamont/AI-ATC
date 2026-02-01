"""
Tests for VFR support module.
"""

import pytest
import numpy as np
from vfr_support import (
    FlightFollowingState,
    VFRFlightType,
    VFRCharacteristics,
    VFR_PROFILES,
    VFRFlightFollowingService,
    VFRTrafficPattern,
    VFRRewardCalculator,
    VFRScenarioGenerator,
)


class TestVFRCharacteristics:
    """Test VFR aircraft characteristics."""

    def test_vfr_profile_general_aviation(self):
        """Test general aviation VFR profile."""
        profile = VFR_PROFILES[VFRFlightType.GENERAL_AVIATION]

        assert profile.flight_type == VFRFlightType.GENERAL_AVIATION
        assert profile.typical_cruise_speed_kts == 100.0
        assert profile.typical_altitude_ft == 3000.0
        assert profile.max_altitude_ft == 8000.0

    def test_vfr_profile_commuter(self):
        """Test commuter VFR profile."""
        profile = VFR_PROFILES[VFRFlightType.COMMUTER]

        assert profile.typical_cruise_speed_kts == 120.0
        assert profile.typical_altitude_ft == 5000.0

    def test_vfr_profile_business_jet(self):
        """Test business jet VFR profile."""
        profile = VFR_PROFILES[VFRFlightType.BUSINESS_JET]

        assert profile.typical_cruise_speed_kts == 200.0
        assert profile.typical_altitude_ft == 8000.0
        # Business jets can go higher
        assert profile.max_altitude_ft > 10000.0

    def test_all_vfr_profiles_valid(self):
        """Test that all VFR profiles are properly defined."""
        for flight_type, profile in VFR_PROFILES.items():
            assert isinstance(profile, VFRCharacteristics)
            assert profile.typical_cruise_speed_kts > 0
            assert profile.typical_altitude_ft > 0
            assert profile.max_altitude_ft > profile.typical_altitude_ft


class TestVFRFlightFollowingService:
    """Test VFR flight following service."""

    def test_service_initialization(self):
        """Test flight following service creation."""
        ffs = VFRFlightFollowingService()

        assert len(ffs.active_sessions) == 0
        assert ffs.separation_buffer_nm == 2.0

    def test_request_flight_following(self):
        """Test requesting flight following."""
        ffs = VFRFlightFollowingService()

        result = ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)

        assert result is True
        assert 1 in ffs.active_sessions
        assert ffs.active_sessions[1]['state'] == FlightFollowingState.ACTIVE

    def test_request_duplicate_flight_following(self):
        """Test requesting flight following twice."""
        ffs = VFRFlightFollowingService()

        ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)
        result = ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)

        assert result is False

    def test_terminate_flight_following(self):
        """Test terminating flight following."""
        ffs = VFRFlightFollowingService()

        ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)
        result = ffs.terminate_flight_following(1)

        assert result is True
        assert ffs.active_sessions[1]['state'] == FlightFollowingState.TERMINATED

    def test_terminate_nonexistent_flight_following(self):
        """Test terminating nonexistent flight following."""
        ffs = VFRFlightFollowingService()

        result = ffs.terminate_flight_following(999)

        assert result is False

    def test_is_receiving_flight_following(self):
        """Test checking flight following status."""
        ffs = VFRFlightFollowingService()

        assert not ffs.is_receiving_flight_following(1)

        ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)
        assert ffs.is_receiving_flight_following(1)

        ffs.terminate_flight_following(1)
        assert not ffs.is_receiving_flight_following(1)

    def test_separation_requirement_vfr_ifr(self):
        """Test separation requirement for VFR/IFR."""
        ffs = VFRFlightFollowingService()

        separation = ffs.get_separation_requirement(vfr_aircraft=True, ifr_aircraft=True)

        assert separation == 2.0

    def test_separation_requirement_vfr_vfr(self):
        """Test separation requirement for VFR/VFR."""
        ffs = VFRFlightFollowingService()

        separation = ffs.get_separation_requirement(vfr_aircraft=True, ifr_aircraft=False)

        assert separation == 1.0


class TestVFRTrafficPattern:
    """Test VFR traffic pattern generation."""

    def test_downwind_entry_generation(self):
        """Test generating downwind entry."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        position, heading = VFRTrafficPattern.generate_downwind_entry(
            airport_pos,
            270.0,  # RWY 27
            downwind_distance_nm=1.5,
            entry_altitude_ft=1000.0,
        )

        assert isinstance(position, np.ndarray)
        assert isinstance(heading, float)
        # Should be ~1.5 NM away
        distance = np.linalg.norm(position - airport_pos)
        assert 1.0 < distance < 2.0

    def test_base_entry_generation(self):
        """Test generating base leg entry."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        position, heading = VFRTrafficPattern.generate_base_entry(
            airport_pos,
            270.0,
            base_distance_nm=1.0,
            entry_altitude_ft=800.0,
        )

        assert isinstance(position, np.ndarray)
        assert isinstance(heading, float)
        distance = np.linalg.norm(position - airport_pos)
        assert 0.5 < distance < 1.5

    def test_straight_in_visual_entry(self):
        """Test generating straight-in visual approach."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        position, heading = VFRTrafficPattern.generate_straight_in_visual(
            airport_pos,
            270.0,
            distance_nm=2.0,
            entry_altitude_ft=1500.0,
        )

        assert isinstance(position, np.ndarray)
        assert isinstance(heading, float)
        distance = np.linalg.norm(position - airport_pos)
        assert distance > 1.0

    def test_entry_patterns_are_different(self):
        """Test that different patterns produce different positions."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        downwind, _ = VFRTrafficPattern.generate_downwind_entry(airport_pos, 270.0)
        base, _ = VFRTrafficPattern.generate_base_entry(airport_pos, 270.0)
        straight_in, _ = VFRTrafficPattern.generate_straight_in_visual(airport_pos, 270.0)

        # All positions should be different
        assert not np.allclose(downwind, base)
        assert not np.allclose(downwind, straight_in)
        assert not np.allclose(base, straight_in)


class TestVFRRewardCalculator:
    """Test VFR reward calculation."""

    def test_calculator_initialization(self):
        """Test creating reward calculator."""
        calc = VFRRewardCalculator()

        assert isinstance(calc, VFRRewardCalculator)
        assert calc.vfr_altitude_penalty == -0.5

    def test_altitude_penalty_below_10k(self):
        """Test no penalty for altitude below 10k."""
        calc = VFRRewardCalculator()

        reward = calc.calculate_vfr_reward(
            aircraft_altitude_ft=8000.0,
            distance_to_airport_nm=5.0,
            is_on_visual_approach=False,
            is_within_separation=True,
        )

        assert reward >= 0.0

    def test_altitude_penalty_above_10k(self):
        """Test penalty for altitude above 10k."""
        calc = VFRRewardCalculator()

        reward = calc.calculate_vfr_reward(
            aircraft_altitude_ft=11000.0,
            distance_to_airport_nm=5.0,
            is_on_visual_approach=False,
            is_within_separation=True,
        )

        # Strong penalty for VFR above 10k should result in negative reward
        # or at least significantly lower than at proper altitude
        reward_at_proper_alt = calc.calculate_vfr_reward(
            aircraft_altitude_ft=8000.0,
            distance_to_airport_nm=5.0,
            is_on_visual_approach=False,
            is_within_separation=True,
        )

        assert reward < reward_at_proper_alt

    def test_visual_approach_bonus(self):
        """Test bonus for visual approach."""
        calc = VFRRewardCalculator()

        reward_visual = calc.calculate_vfr_reward(
            aircraft_altitude_ft=1500.0,
            distance_to_airport_nm=3.0,
            is_on_visual_approach=True,
            is_within_separation=True,
            curriculum_stage=0,
        )

        reward_no_visual = calc.calculate_vfr_reward(
            aircraft_altitude_ft=1500.0,
            distance_to_airport_nm=3.0,
            is_on_visual_approach=False,
            is_within_separation=True,
            curriculum_stage=0,
        )

        assert reward_visual > reward_no_visual

    def test_separation_violation_penalty(self):
        """Test penalty for separation violation."""
        calc = VFRRewardCalculator()

        reward_violation = calc.calculate_vfr_reward(
            aircraft_altitude_ft=5000.0,
            distance_to_airport_nm=5.0,
            is_on_visual_approach=False,
            is_within_separation=False,
        )

        reward_safe = calc.calculate_vfr_reward(
            aircraft_altitude_ft=5000.0,
            distance_to_airport_nm=5.0,
            is_on_visual_approach=False,
            is_within_separation=True,
        )

        assert reward_violation < reward_safe

    def test_vfr_ifr_interaction_reward(self):
        """Test VFR/IFR interaction reward."""
        # Violation
        reward_violation = VFRRewardCalculator.calculate_vfr_ifr_interaction_reward(
            vfr_distance_to_ifr_nm=1.0,
            separation_required_nm=2.0,
        )
        assert reward_violation == -5.0

        # Warning zone
        reward_warning = VFRRewardCalculator.calculate_vfr_ifr_interaction_reward(
            vfr_distance_to_ifr_nm=2.5,
            separation_required_nm=2.0,
        )
        assert reward_warning == -1.0

        # Safe
        reward_safe = VFRRewardCalculator.calculate_vfr_ifr_interaction_reward(
            vfr_distance_to_ifr_nm=4.0,
            separation_required_nm=2.0,
        )
        assert reward_safe == 0.0


class TestVFRScenarioGenerator:
    """Test VFR scenario generation."""

    def test_scenario_generation_vfr_only(self):
        """Test generating VFR-only scenario."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
            num_vfr_aircraft=3,
            num_ifr_aircraft=0,
            airport_position_nm=airport_pos,
            runway_heading_deg=270.0,
        )

        assert len(scenario) == 3
        for ac in scenario:
            assert ac['is_vfr'] is True

    def test_scenario_generation_ifr_only(self):
        """Test generating IFR-only scenario."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
            num_vfr_aircraft=0,
            num_ifr_aircraft=2,
            airport_position_nm=airport_pos,
            runway_heading_deg=270.0,
        )

        assert len(scenario) == 2
        for ac in scenario:
            assert ac['is_vfr'] is False

    def test_scenario_generation_mixed(self):
        """Test generating mixed VFR/IFR scenario."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
            num_vfr_aircraft=3,
            num_ifr_aircraft=2,
            airport_position_nm=airport_pos,
            runway_heading_deg=270.0,
        )

        assert len(scenario) == 5

        vfr_count = sum(1 for ac in scenario if ac['is_vfr'])
        ifr_count = sum(1 for ac in scenario if not ac['is_vfr'])

        assert vfr_count == 3
        assert ifr_count == 2

    def test_scenario_aircraft_have_required_fields(self):
        """Test that generated aircraft have all required fields."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
            num_vfr_aircraft=1,
            num_ifr_aircraft=1,
            airport_position_nm=airport_pos,
            runway_heading_deg=270.0,
        )

        required_fields = [
            'plane_id', 'is_vfr', 'position_nm', 'heading_rad',
            'speed_kts', 'altitude_ft', 'min_speed_kts', 'max_speed_kts'
        ]

        for ac in scenario:
            for field in required_fields:
                assert field in ac

    def test_scenario_vfr_speeds_reasonable(self):
        """Test that VFR speeds are reasonable."""
        airport_pos = np.array([0.0, 0.0], dtype=np.float32)

        scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
            num_vfr_aircraft=4,
            num_ifr_aircraft=0,
            airport_position_nm=airport_pos,
            runway_heading_deg=270.0,
        )

        for ac in scenario:
            if ac['is_vfr']:
                # VFR speeds should be below 250 knots typically
                assert ac['speed_kts'] < 250.0
                # VFR altitudes should be below 10k
                assert ac['altitude_ft'] < 10000.0


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
