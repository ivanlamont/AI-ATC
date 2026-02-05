"""
Live Mode Scenarios for AI-ATC

Integrates real aircraft data into simulation scenarios.

Usage:
    from live_data.live_mode_scenarios import LiveModeScenario

    scenario = LiveModeScenario(airport_code='SFO')
    aircraft = scenario.get_live_aircraft()
"""

import logging
from typing import List, Dict, Optional, Tuple
from dataclasses import dataclass
from datetime import datetime
from enum import Enum

from live_data.adsb_integration import (
    ADSBClient,
    Aircraft,
    filter_aircraft_for_approach,
)

logger = logging.getLogger(__name__)


class SimulationAircraft(Enum):
    """Aircraft source"""
    REAL = "real"  # Real aircraft from ADS-B
    SIMULATED = "simulated"  # Simulated aircraft


@dataclass
class LiveScenarioConfig:
    """Live scenario configuration"""
    airport_code: str
    radius_nm: float = 50
    min_altitude_ft: int = 1000
    max_altitude_ft: int = 50000
    update_interval_seconds: int = 10
    fetch_approaching_only: bool = True
    max_aircraft_count: int = 20
    scale_factor: float = 1.0  # Scale real speed/altitude for gameplay
    show_real_labels: bool = True


class LiveModeScenario:
    """Manages live aircraft scenario"""

    def __init__(self, config: LiveScenarioConfig):
        """Initialize live mode scenario"""
        self.config = config
        self.client = ADSBClient()
        self.last_update = None
        self.aircraft_cache: List[Aircraft] = []
        self.aircraft_source_map: Dict[str, SimulationAircraft] = {}

        # Airport positions
        self.airport_positions = {
            'SFO': (37.6213, -122.3790),
            'LAX': (33.9425, -118.4081),
            'JFK': (40.6413, -73.7781),
            'ORD': (41.9742, -87.9073),
            'ATL': (33.6407, -84.4277),
        }

        if config.airport_code not in self.airport_positions:
            raise ValueError(f"Unknown airport: {config.airport_code}")

        logger.info(f"Live mode scenario initialized for {config.airport_code}")

    def get_live_aircraft(self, force_refresh: bool = False) -> List[Aircraft]:
        """Get live aircraft for scenario"""
        # Check if update needed
        if not force_refresh and self.last_update:
            age = (datetime.utcnow() - self.last_update).total_seconds()
            if age < self.config.update_interval_seconds:
                return self.aircraft_cache

        try:
            # Fetch aircraft from ADSBexchange
            airport_lat, airport_lon = self.airport_positions[self.config.airport_code]

            aircraft = self.client.get_aircraft_by_region(
                lat=airport_lat,
                lon=airport_lon,
                radius_nm=self.config.radius_nm,
                min_altitude_ft=self.config.min_altitude_ft,
                max_altitude_ft=self.config.max_altitude_ft,
            )

            # Filter for approaching if requested
            if self.config.fetch_approaching_only:
                aircraft = filter_aircraft_for_approach(
                    aircraft,
                    target_airport_lat=airport_lat,
                    target_airport_lon=airport_lon,
                    max_distance_nm=30,
                )

            # Limit aircraft count
            if len(aircraft) > self.config.max_aircraft_count:
                # Sort by altitude descending (higher priority to lower aircraft)
                aircraft = sorted(aircraft, key=lambda x: x.altitude_ft)[:self.config.max_aircraft_count]

            # Scale aircraft parameters for gameplay
            scaled_aircraft = [self._scale_aircraft(ac) for ac in aircraft]

            # Update cache
            self.aircraft_cache = scaled_aircraft
            self.last_update = datetime.utcnow()

            # Track source
            for ac in scaled_aircraft:
                self.aircraft_source_map[ac.callsign] = SimulationAircraft.REAL

            logger.info(f"Live update: {len(scaled_aircraft)} aircraft for {self.config.airport_code}")
            return scaled_aircraft

        except Exception as e:
            logger.error(f"Error fetching live aircraft: {e}")
            return self.aircraft_cache

    def _scale_aircraft(self, aircraft: Aircraft) -> Aircraft:
        """Apply scaling to aircraft parameters for gameplay"""
        # Scale ground speed for gameplay
        scaled_speed = int(aircraft.ground_speed_kts * self.config.scale_factor)

        # Altitude scaling (optional - usually keep real)
        scaled_altitude = int(aircraft.altitude_ft * self.config.scale_factor)

        # Create scaled copy
        scaled = Aircraft(
            icao=aircraft.icao,
            callsign=aircraft.callsign,
            latitude=aircraft.latitude,
            longitude=aircraft.longitude,
            altitude_ft=scaled_altitude,
            ground_speed_kts=scaled_speed,
            track_deg=aircraft.track_deg,
            vertical_speed_fpm=aircraft.vertical_speed_fpm,
            squawk=aircraft.squawk,
            aircraft_type=aircraft.aircraft_type,
            registration=aircraft.registration,
            operator=aircraft.operator,
            destination=aircraft.destination,
            origin=aircraft.origin,
            last_update_time=aircraft.last_update_time,
        )

        return scaled

    def get_aircraft_info(self, callsign: str) -> Optional[Dict]:
        """Get detailed info about live aircraft"""
        for ac in self.aircraft_cache:
            if ac.callsign == callsign:
                return {
                    **ac.to_dict(),
                    'source': self.aircraft_source_map.get(callsign, 'unknown'),
                    'distance_to_airport_nm': self._get_distance_to_airport(ac),
                    'time_to_arrival_minutes': self._estimate_arrival_time(ac),
                }

        return None

    def _get_distance_to_airport(self, aircraft: Aircraft) -> float:
        """Calculate distance from aircraft to airport"""
        from live_data.adsb_integration import _calculate_distance

        airport_lat, airport_lon = self.airport_positions[self.config.airport_code]
        return _calculate_distance(
            aircraft.latitude, aircraft.longitude,
            airport_lat, airport_lon
        )

    def _estimate_arrival_time(self, aircraft: Aircraft) -> Optional[float]:
        """Estimate time to arrival in minutes"""
        if aircraft.ground_speed_kts == 0:
            return None

        distance = self._get_distance_to_airport(aircraft)
        time_hours = distance / aircraft.ground_speed_kts
        return time_hours * 60

    def add_simulated_aircraft(self, aircraft: Aircraft) -> None:
        """Add simulated aircraft to scenario"""
        self.aircraft_cache.append(aircraft)
        self.aircraft_source_map[aircraft.callsign] = SimulationAircraft.SIMULATED
        logger.info(f"Added simulated aircraft: {aircraft.callsign}")

    def remove_aircraft(self, callsign: str) -> bool:
        """Remove aircraft from scenario"""
        initial_count = len(self.aircraft_cache)

        self.aircraft_cache = [ac for ac in self.aircraft_cache if ac.callsign != callsign]

        if len(self.aircraft_cache) < initial_count:
            self.aircraft_source_map.pop(callsign, None)
            logger.info(f"Removed aircraft: {callsign}")
            return True

        return False

    def get_statistics(self) -> Dict:
        """Get scenario statistics"""
        real_count = sum(
            1 for ac in self.aircraft_cache
            if self.aircraft_source_map.get(ac.callsign) == SimulationAircraft.REAL
        )

        simulated_count = len(self.aircraft_cache) - real_count

        airlines = {}
        for ac in self.aircraft_cache:
            airlines[ac.operator] = airlines.get(ac.operator, 0) + 1

        avg_altitude = sum(ac.altitude_ft for ac in self.aircraft_cache) / len(self.aircraft_cache) \
            if self.aircraft_cache else 0

        avg_speed = sum(ac.ground_speed_kts for ac in self.aircraft_cache) / len(self.aircraft_cache) \
            if self.aircraft_cache else 0

        return {
            'total_aircraft': len(self.aircraft_cache),
            'real_aircraft': real_count,
            'simulated_aircraft': simulated_count,
            'airport': self.config.airport_code,
            'airlines': airlines,
            'average_altitude_ft': int(avg_altitude),
            'average_speed_kts': int(avg_speed),
            'last_update': self.last_update.isoformat() if self.last_update else None,
            'cache_status': self.client.get_cache_status(),
        }

    def convert_to_simulation_format(self) -> Dict:
        """Convert live aircraft to simulation format"""
        return {
            'airport': self.config.airport_code,
            'timestamp': datetime.utcnow().isoformat(),
            'mode': 'live',
            'aircraft': [
                {
                    'callsign': ac.callsign,
                    'type': ac.aircraft_type,
                    'position': {
                        'latitude': ac.latitude,
                        'longitude': ac.longitude,
                        'altitude_ft': ac.altitude_ft,
                    },
                    'velocity': {
                        'ground_speed_kts': ac.ground_speed_kts,
                        'track_deg': ac.track_deg,
                        'vertical_speed_fpm': ac.vertical_speed_fpm,
                    },
                    'source': self.aircraft_source_map.get(ac.callsign, 'unknown').value,
                    'real_data': {
                        'icao': ac.icao,
                        'squawk': ac.squawk,
                        'registration': ac.registration,
                        'operator': ac.operator,
                        'origin': ac.origin,
                        'destination': ac.destination,
                    } if self.aircraft_source_map.get(ac.callsign) == SimulationAircraft.REAL else None,
                }
                for ac in self.aircraft_cache
            ]
        }


class LiveModeManager:
    """Manages multiple live mode scenarios"""

    def __init__(self):
        """Initialize manager"""
        self.scenarios: Dict[str, LiveModeScenario] = {}

    def create_scenario(self, config: LiveScenarioConfig) -> LiveModeScenario:
        """Create new live scenario"""
        scenario = LiveModeScenario(config)
        self.scenarios[config.airport_code] = scenario
        logger.info(f"Created live scenario for {config.airport_code}")
        return scenario

    def get_scenario(self, airport_code: str) -> Optional[LiveModeScenario]:
        """Get existing scenario"""
        return self.scenarios.get(airport_code)

    def list_scenarios(self) -> List[str]:
        """List all active scenarios"""
        return list(self.scenarios.keys())

    def get_all_statistics(self) -> Dict:
        """Get statistics for all scenarios"""
        return {
            airport: scenario.get_statistics()
            for airport, scenario in self.scenarios.items()
        }


def create_live_scenario_for_airport(
    airport_code: str,
    fetch_approaching_only: bool = True,
) -> LiveModeScenario:
    """Helper to create live scenario for airport"""
    config = LiveScenarioConfig(
        airport_code=airport_code,
        fetch_approaching_only=fetch_approaching_only,
    )
    return LiveModeScenario(config)


if __name__ == '__main__':
    # Example usage
    logging.basicConfig(level=logging.INFO)

    # Create live scenario for SFO
    print("Creating live scenario for SFO...")
    scenario = create_live_scenario_for_airport('SFO')

    # Get aircraft
    print("Fetching live aircraft...")
    aircraft = scenario.get_live_aircraft()

    print(f"Found {len(aircraft)} aircraft\n")

    # Display statistics
    print("Scenario Statistics:")
    stats = scenario.get_statistics()
    for key, value in stats.items():
        if key != 'cache_status':
            print(f"  {key}: {value}")

    # Display aircraft
    print("\nAircraft:")
    for ac in aircraft[:10]:
        print(f"  {ac.callsign}: {ac.aircraft_type} at {ac.altitude_ft} ft, {ac.ground_speed_kts} kt")
