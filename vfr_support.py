"""
VFR (Visual Flight Rules) support for AI-ATC.
Implements VFR-specific behaviors, flight following, and traffic management.
"""

import numpy as np
from dataclasses import dataclass
from typing import List, Optional, Tuple
from enum import Enum


class FlightFollowingState(Enum):
    """States for VFR flight following service."""
    REQUESTING = "requesting"  # VFR aircraft requesting flight following
    ACTIVE = "active"          # Flight following is active
    TERMINATED = "terminated"  # Pilot terminated flight following
    NOT_AVAILABLE = "not_available"  # Service not available


class VFRFlightType(Enum):
    """Types of VFR flights."""
    GENERAL_AVIATION = "general_aviation"  # Small GA aircraft
    COMMUTER = "commuter"                   # Commuter aircraft
    BUSINESS_JET = "business_jet"           # Business jets
    CARGO = "cargo"                          # Cargo/freight


@dataclass
class VFRCharacteristics:
    """VFR-specific aircraft characteristics."""
    flight_type: VFRFlightType
    typical_cruise_speed_kts: float  # Typical cruise speed
    typical_altitude_ft: float       # Typical VFR altitude
    max_altitude_ft: float = 10000.0  # VFR altitude ceiling
    min_visual_ceiling_ft: float = 1000.0  # Minimum ceiling for VFR
    min_visibility_sm: float = 3.0   # Minimum visibility
    request_flight_following: bool = True
    uses_standard_routes: bool = False  # VFR typically don't use airways
    separation_buffer_nm: float = 2.0  # Slightly larger buffer for VFR


# Predefined VFR profiles
VFR_PROFILES = {
    VFRFlightType.GENERAL_AVIATION: VFRCharacteristics(
        flight_type=VFRFlightType.GENERAL_AVIATION,
        typical_cruise_speed_kts=100.0,
        typical_altitude_ft=3000.0,
        max_altitude_ft=8000.0,
    ),
    VFRFlightType.COMMUTER: VFRCharacteristics(
        flight_type=VFRFlightType.COMMUTER,
        typical_cruise_speed_kts=120.0,
        typical_altitude_ft=5000.0,
        max_altitude_ft=9000.0,
    ),
    VFRFlightType.BUSINESS_JET: VFRCharacteristics(
        flight_type=VFRFlightType.BUSINESS_JET,
        typical_cruise_speed_kts=200.0,
        typical_altitude_ft=8000.0,
        max_altitude_ft=15000.0,  # Some business jets can go higher
    ),
    VFRFlightType.CARGO: VFRCharacteristics(
        flight_type=VFRFlightType.CARGO,
        typical_cruise_speed_kts=140.0,
        typical_altitude_ft=4000.0,
        max_altitude_ft=9000.0,
    ),
}


class VFRFlightFollowingService:
    """Manages VFR flight following service."""

    def __init__(self):
        self.active_sessions: dict = {}  # plane_id -> session info
        self.separation_buffer_nm = 2.0  # VFR separation buffer

    def request_flight_following(self, plane_id: int, aircraft_type: VFRFlightType) -> bool:
        """Request flight following service."""
        if plane_id not in self.active_sessions:
            self.active_sessions[plane_id] = {
                'state': FlightFollowingState.ACTIVE,
                'type': aircraft_type,
                'requested_alt': None,
                'last_update': 0.0,
            }
            return True
        return False

    def terminate_flight_following(self, plane_id: int) -> bool:
        """Terminate flight following service."""
        if plane_id in self.active_sessions:
            self.active_sessions[plane_id]['state'] = FlightFollowingState.TERMINATED
            return True
        return False

    def is_receiving_flight_following(self, plane_id: int) -> bool:
        """Check if aircraft is receiving active flight following."""
        session = self.active_sessions.get(plane_id)
        return session is not None and session['state'] == FlightFollowingState.ACTIVE

    def get_separation_requirement(self, vfr_aircraft: bool, ifr_aircraft: bool = False) -> float:
        """Get separation requirement between aircraft types."""
        if vfr_aircraft and ifr_aircraft:
            # VFR/IFR separation (typically 1000 ft vertical or 2 nm horizontal)
            return 2.0
        elif vfr_aircraft and vfr_aircraft:
            # VFR/VFR separation (typically 1 nm visual separation)
            return 1.0
        else:
            # IFR/IFR separation (standard 1000 ft or 3 nm)
            return 3.0


class VFRTrafficPattern:
    """Generates VFR traffic patterns and approach vectors."""

    @staticmethod
    def generate_downwind_entry(
        airport_position_nm: np.ndarray,
        runway_heading_deg: float,
        downwind_distance_nm: float = 1.5,
        entry_altitude_ft: float = 1000.0,
    ) -> Tuple[np.ndarray, float]:
        """
        Generate a standard VFR downwind entry point.

        Args:
            airport_position_nm: Airport position in NM
            runway_heading_deg: Runway heading in degrees
            downwind_distance_nm: Distance from runway on downwind leg
            entry_altitude_ft: Altitude for entry

        Returns:
            Tuple of (position_nm, heading_rad)
        """
        runway_rad = np.deg2rad(runway_heading_deg)

        # Downwind is parallel to runway, opposite direction, 1.5 nm out
        perpendicular_rad = runway_rad + np.pi / 2

        position = airport_position_nm + np.array(
            [np.cos(perpendicular_rad), np.sin(perpendicular_rad)],
            dtype=np.float32
        ) * downwind_distance_nm

        # Aircraft heading on downwind
        heading = runway_rad + np.pi  # Opposite to runway heading

        return position, heading

    @staticmethod
    def generate_base_entry(
        airport_position_nm: np.ndarray,
        runway_heading_deg: float,
        base_distance_nm: float = 1.0,
        entry_altitude_ft: float = 800.0,
    ) -> Tuple[np.ndarray, float]:
        """Generate a standard VFR base leg entry point."""
        runway_rad = np.deg2rad(runway_heading_deg)

        # Base leg perpendicular to runway
        base_vector = np.array(
            [np.cos(runway_rad), np.sin(runway_rad)],
            dtype=np.float32
        ) * base_distance_nm

        position = airport_position_nm + base_vector

        # Heading towards runway
        heading = runway_rad

        return position, heading

    @staticmethod
    def generate_straight_in_visual(
        airport_position_nm: np.ndarray,
        runway_heading_deg: float,
        distance_nm: float = 2.0,
        entry_altitude_ft: float = 1500.0,
    ) -> Tuple[np.ndarray, float]:
        """Generate a straight-in visual approach entry."""
        runway_rad = np.deg2rad(runway_heading_deg)

        # Straight in on final approach
        inbound_vector = -np.array(
            [np.cos(runway_rad), np.sin(runway_rad)],
            dtype=np.float32
        ) * distance_nm

        position = airport_position_nm + inbound_vector

        # Heading towards runway
        heading = runway_rad

        return position, heading


class VFRRewardCalculator:
    """Calculates rewards for VFR operations."""

    def __init__(self):
        self.vfr_altitude_penalty = -0.5  # Penalty for going too high
        self.visual_approach_bonus = 1.0   # Bonus for visual approach
        self.flight_following_bonus = 0.5  # Bonus for maintaining flight following

    def calculate_vfr_reward(
        self,
        aircraft_altitude_ft: float,
        distance_to_airport_nm: float,
        is_on_visual_approach: bool,
        is_within_separation: bool,
        curriculum_stage: int = 0,
    ) -> float:
        """Calculate reward for VFR aircraft operations."""
        reward = 0.0

        # Altitude management reward (critical)
        if aircraft_altitude_ft > 10000.0:
            reward -= 1.0  # Strong penalty for VFR above 10k
        elif aircraft_altitude_ft > 8000.0:
            reward -= self.vfr_altitude_penalty  # Penalty for being high
        else:
            reward += 0.2  # Small bonus for correct altitude

        # Visual approach bonus
        if is_on_visual_approach and distance_to_airport_nm < 5.0:
            reward += self.visual_approach_bonus * (1.0 - distance_to_airport_nm / 5.0)

        # Separation maintenance reward
        if not is_within_separation:
            reward -= 2.0  # Significant penalty for separation violation

        # Stage-based shaping (only add positive reward)
        if curriculum_stage >= 1:
            stage_bonus = 0.3 * (1.0 - min(distance_to_airport_nm / 10.0, 1.0))
            # Don't add bonus if aircraft is too high
            if aircraft_altitude_ft <= 10000.0:
                reward += stage_bonus

        return reward

    @staticmethod
    def calculate_vfr_ifr_interaction_reward(
        vfr_distance_to_ifr_nm: float,
        separation_required_nm: float,
    ) -> float:
        """Calculate reward for proper VFR/IFR separation."""
        if vfr_distance_to_ifr_nm < separation_required_nm:
            return -5.0  # Major penalty for separation violation
        elif vfr_distance_to_ifr_nm < separation_required_nm * 1.5:
            return -1.0  # Warning penalty
        else:
            return 0.0  # OK


class VFRScenarioGenerator:
    """Generates VFR traffic scenarios."""

    @staticmethod
    def generate_vfr_traffic_scenario(
        num_vfr_aircraft: int,
        num_ifr_aircraft: int,
        airport_position_nm: np.ndarray,
        runway_heading_deg: float,
    ) -> List[dict]:
        """
        Generate a mixed VFR/IFR traffic scenario.

        Returns:
            List of aircraft spawn configurations
        """
        aircraft_configs = []

        # Generate VFR aircraft
        for i in range(num_vfr_aircraft):
            vfr_type = list(VFRFlightType)[i % len(VFRFlightType)]
            profile = VFR_PROFILES[vfr_type]

            # Randomly choose VFR entry pattern
            pattern_choice = i % 3
            if pattern_choice == 0:
                # Downwind entry
                pos, heading = VFRTrafficPattern.generate_downwind_entry(
                    airport_position_nm,
                    runway_heading_deg,
                )
            elif pattern_choice == 1:
                # Base entry
                pos, heading = VFRTrafficPattern.generate_base_entry(
                    airport_position_nm,
                    runway_heading_deg,
                )
            else:
                # Straight in visual
                pos, heading = VFRTrafficPattern.generate_straight_in_visual(
                    airport_position_nm,
                    runway_heading_deg,
                )

            aircraft_configs.append({
                'plane_id': i,
                'is_vfr': True,
                'vfr_type': vfr_type,
                'position_nm': pos,
                'heading_rad': heading,
                'speed_kts': profile.typical_cruise_speed_kts,
                'altitude_ft': profile.typical_altitude_ft,
                'min_speed_kts': profile.typical_cruise_speed_kts - 20.0,
                'max_speed_kts': profile.typical_cruise_speed_kts + 30.0,
            })

        # Generate IFR aircraft (standard approach)
        for i in range(num_ifr_aircraft):
            ifr_id = num_vfr_aircraft + i
            aircraft_configs.append({
                'plane_id': ifr_id,
                'is_vfr': False,
                'position_nm': airport_position_nm + np.array([-15.0, 5.0], dtype=np.float32),
                'heading_rad': np.deg2rad(runway_heading_deg),
                'speed_kts': 180.0,
                'altitude_ft': 6000.0 - (i * 1000.0),
                'min_speed_kts': 140.0,
                'max_speed_kts': 200.0,
            })

        return aircraft_configs


if __name__ == "__main__":
    # Test VFR support
    print("Testing VFR Support Module")
    print("=" * 50)

    # Test flight following service
    ffs = VFRFlightFollowingService()
    print(f"Flight following requested: {ffs.request_flight_following(1, VFRFlightType.GENERAL_AVIATION)}")
    print(f"Is receiving flight following: {ffs.is_receiving_flight_following(1)}")

    # Test traffic pattern generation
    airport_pos = np.array([0.0, 0.0], dtype=np.float32)
    downwind_pos, downwind_hdg = VFRTrafficPattern.generate_downwind_entry(
        airport_pos,
        270.0,  # RWY 27
    )
    print(f"\nDownwind entry position: {downwind_pos}")
    print(f"Downwind entry heading: {np.rad2deg(downwind_hdg):.1f}°")

    # Test scenario generation
    scenario = VFRScenarioGenerator.generate_vfr_traffic_scenario(
        num_vfr_aircraft=3,
        num_ifr_aircraft=2,
        airport_position_nm=airport_pos,
        runway_heading_deg=270.0,
    )
    print(f"\nGenerated {len(scenario)} aircraft for scenario")
    for ac in scenario:
        print(f"  Plane {ac['plane_id']}: VFR={ac['is_vfr']}, Speed={ac['speed_kts']:.0f}kts")
