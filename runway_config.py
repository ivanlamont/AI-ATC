"""
Runway configuration management system.
Handles dynamic runway changes, wind-based runway selection, and multi-runway operations.
"""

import numpy as np
from dataclasses import dataclass
from typing import List, Optional, Tuple, Dict
from enum import Enum
from runway import Runway
from airport import Airport


class RunwayOrientation(Enum):
    """Runway orientation categories."""
    NORTH = "north"           # 0-45° or 315-360°
    NORTHEAST = "northeast"   # 45-90°
    EAST = "east"             # 90-135°
    SOUTHEAST = "southeast"   # 135-180°
    SOUTH = "south"           # 180-225°
    SOUTHWEST = "southwest"   # 225-270°
    WEST = "west"             # 270-315°
    NORTHWEST = "northwest"   # 315-360°


class RunwayStatus(Enum):
    """Runway operational status."""
    ACTIVE = "active"
    INACTIVE = "inactive"
    CLOSED = "closed"
    MAINTENANCE = "maintenance"


@dataclass
class WindConditions:
    """Current wind conditions."""
    wind_speed_kts: float     # Knots
    wind_direction_deg: float # Degrees (where wind comes FROM)
    wind_gust_kts: float = 0.0  # Wind gust speed

    def get_crosswind_component(self, runway_heading_deg: float) -> float:
        """
        Calculate crosswind component for a runway.

        Args:
            runway_heading_deg: Runway heading (direction aircraft fly toward)

        Returns:
            Crosswind component in knots (positive = from right)
        """
        # Convert wind direction to headwind/crosswind relative to runway
        relative_angle = runway_heading_deg - self.wind_direction_deg
        relative_angle_rad = np.deg2rad(relative_angle)

        # Crosswind component
        crosswind = self.wind_speed_kts * np.sin(relative_angle_rad)
        return crosswind

    def get_headwind_component(self, runway_heading_deg: float) -> float:
        """
        Calculate headwind component for a runway.

        Returns:
            Headwind component in knots (positive = headwind, negative = tailwind)
        """
        relative_angle = runway_heading_deg - self.wind_direction_deg
        relative_angle_rad = np.deg2rad(relative_angle)

        # Headwind component
        headwind = self.wind_speed_kts * np.cos(relative_angle_rad)
        return headwind


@dataclass
class RunwayConfig:
    """Configuration for a single runway."""
    runway_id: str  # e.g., "RWY 27L"
    runway_heading_deg: float
    length_ft: float  # Runway length
    width_ft: float   # Runway width
    surface_type: str = "asphalt"  # asphalt, concrete, grass
    faf_distance_nm: float = 6.0

    # Operational limits
    max_headwind_kts: float = 45.0  # Maximum acceptable headwind
    max_crosswind_kts: float = 15.0  # Maximum acceptable crosswind
    max_tailwind_kts: float = 10.0   # Maximum acceptable tailwind

    # Status
    status: RunwayStatus = RunwayStatus.ACTIVE
    closed_until_time: Optional[float] = None  # Simulation time when runway reopens

    def is_operational(self, current_time: Optional[float] = None) -> bool:
        """Check if runway is operational."""
        if self.status == RunwayStatus.CLOSED:
            if self.closed_until_time is not None and current_time is not None:
                return current_time >= self.closed_until_time
            return False
        return self.status == RunwayStatus.ACTIVE

    def can_accept_aircraft(self, wind_conditions: WindConditions) -> Tuple[bool, Optional[str]]:
        """
        Check if runway can accept aircraft given wind conditions.

        Returns:
            Tuple of (can_accept, reason_if_not)
        """
        if not self.is_operational():
            return False, f"{self.runway_id} is not operational"

        crosswind = abs(wind_conditions.get_crosswind_component(self.runway_heading_deg))
        headwind = wind_conditions.get_headwind_component(self.runway_heading_deg)

        if crosswind > self.max_crosswind_kts:
            return False, f"Crosswind {crosswind:.1f}kts exceeds limit {self.max_crosswind_kts}kts"

        if headwind > self.max_headwind_kts:
            return False, f"Headwind {headwind:.1f}kts exceeds limit {self.max_headwind_kts}kts"

        if headwind < -self.max_tailwind_kts:
            return False, f"Tailwind {-headwind:.1f}kts exceeds limit {self.max_tailwind_kts}kts"

        return True, None

    def get_suitability_score(self, wind_conditions: WindConditions) -> float:
        """
        Calculate suitability score for runway (0-100).
        Higher is better. Negative if unsuitable.
        """
        if not self.is_operational():
            return -100.0

        can_accept, _ = self.can_accept_aircraft(wind_conditions)
        if not can_accept:
            return -50.0

        # Ideal conditions: straight-in landing with minimal crosswind
        crosswind = abs(wind_conditions.get_crosswind_component(self.runway_heading_deg))
        headwind = wind_conditions.get_headwind_component(self.runway_heading_deg)

        # Score based on how close to ideal conditions
        score = 100.0

        # Penalize for crosswind (max penalty 20 points)
        crosswind_penalty = (crosswind / self.max_crosswind_kts) * 20.0
        score -= min(crosswind_penalty, 20.0)

        # Penalize for tailwind (max penalty 30 points)
        if headwind < 0:
            tailwind_penalty = (-headwind / self.max_tailwind_kts) * 30.0
            score -= min(tailwind_penalty, 30.0)
        else:
            # Bonus for headwind (headwind is good for landing)
            headwind_bonus = min(headwind / 10.0, 10.0)
            score += headwind_bonus

        return max(score, 0.0)


class RunwayConfigurationManager:
    """Manages runway configurations and dynamic changes."""

    def __init__(self, airport: Airport):
        self.airport = airport
        self.runways: Dict[str, RunwayConfig] = {}
        self.active_runway: Optional[str] = None
        self.runway_objects: Dict[str, Runway] = {}
        self.wind_conditions = WindConditions(0.0, 0.0)
        self.configuration_history: List[Tuple[float, str, str]] = []  # (time, from_runway, to_runway)
        self.last_config_change_time = 0.0
        self.min_time_between_changes_seconds = 300.0  # Minimum 5 minutes between changes

    def add_runway(self, runway_config: RunwayConfig) -> None:
        """Add a runway to the airport."""
        self.runways[runway_config.runway_id] = runway_config

        # Create runway object
        self.runway_objects[runway_config.runway_id] = Runway(
            airport=self.airport,
            runway_heading_deg=runway_config.runway_heading_deg,
            faf_distance_nm=runway_config.faf_distance_nm,
        )

        # Set first active runway
        if self.active_runway is None:
            self.active_runway = runway_config.runway_id

    def get_active_runway(self) -> Optional[RunwayConfig]:
        """Get currently active runway configuration."""
        if self.active_runway is None:
            return None
        return self.runways.get(self.active_runway)

    def get_active_runway_object(self) -> Optional[Runway]:
        """Get currently active runway object."""
        if self.active_runway is None:
            return None
        return self.runway_objects.get(self.active_runway)

    def update_wind_conditions(
        self,
        wind_speed_kts: float,
        wind_direction_deg: float,
        wind_gust_kts: float = 0.0
    ) -> None:
        """Update current wind conditions."""
        self.wind_conditions = WindConditions(wind_speed_kts, wind_direction_deg, wind_gust_kts)

    def get_best_runway(self) -> Optional[str]:
        """
        Select best runway for current wind conditions.

        Returns:
            Runway ID with highest suitability score
        """
        best_runway = None
        best_score = -100.0

        for runway_id, config in self.runways.items():
            if not config.is_operational():
                continue

            score = config.get_suitability_score(self.wind_conditions)
            if score > best_score:
                best_score = score
                best_runway = runway_id

        return best_runway

    def evaluate_configuration_change(self, current_time: float) -> Tuple[bool, Optional[str], str]:
        """
        Evaluate if runway configuration should change.

        Returns:
            Tuple of (should_change, new_runway, reason)
        """
        # Check if enough time has passed since last change
        if current_time - self.last_config_change_time < self.min_time_between_changes_seconds:
            return False, None, "Minimum time between changes not met"

        current_runway = self.get_active_runway()
        best_runway = self.get_best_runway()

        if current_runway is None or best_runway is None:
            return False, None, "No suitable runways available"

        if best_runway == self.active_runway:
            return False, None, "Current runway is optimal"

        # Check if current runway is no longer suitable
        can_accept, reason = current_runway.can_accept_aircraft(self.wind_conditions)
        if not can_accept:
            return True, best_runway, f"Current runway unsuitable: {reason}"

        # Check if best runway is significantly better
        current_score = current_runway.get_suitability_score(self.wind_conditions)
        best_score = self.runways[best_runway].get_suitability_score(self.wind_conditions)

        if best_score > current_score + 15.0:  # 15 point threshold
            return True, best_runway, f"Better runway available (score {best_score:.0f} vs {current_score:.0f})"

        return False, None, "Current runway acceptable"

    def change_runway_configuration(self, new_runway_id: str, current_time: float) -> Tuple[bool, str]:
        """
        Change runway configuration.

        Returns:
            Tuple of (success, message)
        """
        if new_runway_id not in self.runways:
            return False, f"Runway {new_runway_id} not found"

        if not self.runways[new_runway_id].is_operational():
            return False, f"Runway {new_runway_id} is not operational"

        old_runway = self.active_runway
        self.active_runway = new_runway_id
        self.last_config_change_time = current_time

        if old_runway:
            self.configuration_history.append((current_time, old_runway, new_runway_id))

        return True, f"Runway configuration changed from {old_runway} to {new_runway_id}"

    def close_runway(self, runway_id: str, reopen_at_time: Optional[float] = None) -> None:
        """Close a runway for maintenance or emergency."""
        if runway_id in self.runways:
            self.runways[runway_id].status = RunwayStatus.CLOSED
            self.runways[runway_id].closed_until_time = reopen_at_time

            # Switch to different runway if needed
            if self.active_runway == runway_id:
                best_runway = self.get_best_runway()
                if best_runway:
                    self.active_runway = best_runway

    def reopen_runway(self, runway_id: str) -> None:
        """Reopen a closed runway."""
        if runway_id in self.runways:
            self.runways[runway_id].status = RunwayStatus.ACTIVE
            self.runways[runway_id].closed_until_time = None

    def get_summary(self) -> str:
        """Get summary of current runway configuration."""
        active = self.get_active_runway()
        summary = f"\n{'='*60}\n"
        summary += f"RUNWAY CONFIGURATION\n"
        summary += f"{'='*60}\n"
        summary += f"Active Runway: {self.active_runway}\n"

        if active:
            summary += f"Heading: {active.runway_heading_deg:.0f}°\n"
            summary += f"Length: {active.length_ft:.0f} ft\n"

            can_accept, reason = active.can_accept_aircraft(self.wind_conditions)
            summary += f"Status: {'OPERATIONAL' if can_accept else 'UNSUITABLE'}\n"
            if not can_accept:
                summary += f"  Reason: {reason}\n"

            score = active.get_suitability_score(self.wind_conditions)
            summary += f"Suitability Score: {score:.0f}/100\n"

        summary += f"\nWind: {self.wind_conditions.wind_speed_kts:.1f}kts from {self.wind_conditions.wind_direction_deg:.0f}°\n"
        summary += f"Gust: {self.wind_conditions.wind_gust_kts:.1f}kts\n"

        summary += f"\nAvailable Runways:\n"
        for runway_id, config in self.runways.items():
            status = "ACTIVE" if config.status == RunwayStatus.ACTIVE else "CLOSED"
            score = config.get_suitability_score(self.wind_conditions)
            marker = "→" if runway_id == self.active_runway else " "
            summary += f"  {marker} {runway_id}: {config.runway_heading_deg:.0f}° ({status}) Score: {score:.0f}\n"

        summary += f"{'='*60}\n"
        return summary


if __name__ == "__main__":
    # Test runway configuration management
    print("Testing Runway Configuration Management")
    print("=" * 60)

    # Create airport and manager
    airport = Airport(position_nm=np.array([0.0, 0.0], dtype=np.float32))
    manager = RunwayConfigurationManager(airport)

    # Add runways
    manager.add_runway(RunwayConfig(
        runway_id="RWY 27L",
        runway_heading_deg=270.0,
        length_ft=10000.0,
        width_ft=150.0,
    ))

    manager.add_runway(RunwayConfig(
        runway_id="RWY 27R",
        runway_heading_deg=270.0,
        length_ft=10000.0,
        width_ft=150.0,
    ))

    manager.add_runway(RunwayConfig(
        runway_id="RWY 09L",
        runway_heading_deg=90.0,
        length_ft=8000.0,
        width_ft=150.0,
    ))

    # Test wind scenarios
    scenarios = [
        (10.0, 270.0, "Westerly wind - good for RWY 27"),
        (15.0, 90.0, "Easterly wind - good for RWY 09"),
        (20.0, 180.0, "Southerly wind - crosswind for all"),
    ]

    for wind_speed, wind_dir, description in scenarios:
        print(f"\n{description}")
        manager.update_wind_conditions(wind_speed, wind_dir, 0.0)
        best = manager.get_best_runway()
        print(f"Best runway: {best}")
        print(manager.get_summary())
