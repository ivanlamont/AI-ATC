"""
Scenario Builder for AI-ATC

Builds playable scenarios from airport configurations with difficulty variants.
"""

import json
from typing import Dict, List, Optional
from dataclasses import asdict
from enum import Enum
from scenarios.airport_scenarios import (
    AirportScenario,
    get_airport_scenario,
    MAJOR_AIRPORTS,
    AirportCode,
)


class DifficultyLevel(Enum):
    """Scenario difficulty levels"""
    BEGINNER = "beginner"
    INTERMEDIATE = "intermediate"
    ADVANCED = "advanced"
    EXPERT = "expert"


class ScenarioDifficulty:
    """Difficulty parameters for scenarios"""

    PRESETS = {
        DifficultyLevel.BEGINNER: {
            "aircraft_count": 1,
            "traffic_density": 0.3,
            "weather_severity": 0.2,
            "procedure_complexity": 0.2,
            "wind_change_probability": 0.0,
            "go_around_probability": 0.0,
            "separation_tolerance": 1.5,
            "time_pressure": 0.2,
        },
        DifficultyLevel.INTERMEDIATE: {
            "aircraft_count": 3,
            "traffic_density": 0.6,
            "weather_severity": 0.5,
            "procedure_complexity": 0.5,
            "wind_change_probability": 0.2,
            "go_around_probability": 0.1,
            "separation_tolerance": 1.0,
            "time_pressure": 0.5,
        },
        DifficultyLevel.ADVANCED: {
            "aircraft_count": 5,
            "traffic_density": 0.8,
            "weather_severity": 0.7,
            "procedure_complexity": 0.8,
            "wind_change_probability": 0.4,
            "go_around_probability": 0.2,
            "separation_tolerance": 0.9,
            "time_pressure": 0.75,
        },
        DifficultyLevel.EXPERT: {
            "aircraft_count": 8,
            "traffic_density": 1.0,
            "weather_severity": 0.9,
            "procedure_complexity": 1.0,
            "wind_change_probability": 0.6,
            "go_around_probability": 0.4,
            "separation_tolerance": 0.8,
            "time_pressure": 1.0,
        },
    }


class ScenarioBuilder:
    """Builds scenarios from airport data"""

    def __init__(self):
        self.scenarios = {}

    def build_airport_scenarios(self, airport_code: str) -> Dict[str, Dict]:
        """Build all difficulty variants for an airport"""
        try:
            airport = get_airport_scenario(airport_code)
        except ValueError as e:
            raise ValueError(f"Cannot build scenarios: {e}")

        scenarios = {}

        for difficulty in DifficultyLevel:
            scenario = self._build_single_scenario(airport, difficulty)
            scenarios[difficulty.value] = scenario

        return scenarios

    def _build_single_scenario(
        self,
        airport: AirportScenario,
        difficulty: DifficultyLevel
    ) -> Dict:
        """Build a single scenario for specific difficulty"""
        difficulty_params = ScenarioDifficulty.PRESETS[difficulty]

        # Select weather pattern
        weather = self._select_weather(airport, difficulty_params)

        # Generate traffic
        traffic = self._generate_traffic(airport, difficulty_params)

        # Select active challenge
        challenge = self._select_challenge(airport, difficulty)

        # Select initial runway
        initial_runway = self._select_initial_runway(airport, weather)

        return {
            "airport": airport.airport_code.value,
            "airport_name": airport.name,
            "difficulty": difficulty.value,
            "duration_minutes": self._calculate_duration(difficulty),
            "difficulty_params": difficulty_params,
            "weather": weather,
            "initial_runway": initial_runway,
            "runways": [asdict(r) for r in airport.runways],
            "procedures": [
                {
                    "name": p.name,
                    "type": p.procedure_type,
                    "runway": p.runway,
                    "description": p.description,
                }
                for p in airport.procedures
            ],
            "traffic": traffic,
            "challenge": challenge,
            "objectives": self._build_objectives(airport, difficulty, challenge),
            "success_criteria": self._build_success_criteria(difficulty),
        }

    def _select_weather(self, airport: AirportScenario, params: Dict) -> Dict:
        """Select appropriate weather based on difficulty"""
        if not airport.weather_patterns:
            return {
                "visibility_sm": 10,
                "ceiling_ft": 5000,
                "wind_speed": 8,
                "wind_direction": 270,
            }

        # Select weather based on severity
        severity_threshold = params["weather_severity"]
        pattern_idx = int(len(airport.weather_patterns) * severity_threshold)

        if pattern_idx >= len(airport.weather_patterns):
            pattern_idx = len(airport.weather_patterns) - 1

        return airport.weather_patterns[pattern_idx]

    def _select_initial_runway(self, airport: AirportScenario, weather: Dict) -> str:
        """Select best runway based on wind"""
        best_runway = None
        best_score = float('inf')

        wind_dir = weather.get("wind_direction", 270)

        for runway in airport.runways:
            # Calculate crosswind component
            angle_diff = abs(runway.heading_deg - wind_dir)
            if angle_diff > 180:
                angle_diff = 360 - angle_diff

            crosswind = weather.get("wind_speed", 8) * abs(np.sin(np.radians(angle_diff)))

            # Lower crosswind is better
            if crosswind < best_score:
                best_score = crosswind
                best_runway = runway.runway_id

        return best_runway or airport.runways[0].runway_id

    def _generate_traffic(self, airport: AirportScenario, params: Dict) -> List[Dict]:
        """Generate traffic for scenario"""
        import random

        traffic = []
        aircraft_count = int(params["aircraft_count"])

        # Get typical traffic for airport
        if not airport.typical_traffic:
            return []

        # Scale traffic based on density
        airport_traffic = airport.typical_traffic.copy()

        for airline_config in airport_traffic:
            count = int(airline_config["count"] * params["traffic_density"])

            for i in range(count):
                if len(traffic) >= aircraft_count:
                    break

                traffic.append({
                    "callsign": f"{airline_config['aircraft_id']}{1000 + len(traffic)}",
                    "aircraft_type": airline_config["aircraft_type"],
                    "weight": airline_config.get("weight", "medium"),
                    "spawn_time_minutes": random.uniform(0, 10),
                    "initial_altitude_ft": random.uniform(4000, 8000),
                    "destination_runway": None,  # Will be assigned by controller
                })

            if len(traffic) >= aircraft_count:
                break

        return traffic

    def _select_challenge(
        self,
        airport: AirportScenario,
        difficulty: DifficultyLevel
    ) -> Dict:
        """Select appropriate challenge for difficulty"""
        if not airport.challenges:
            return {}

        challenge_idx = {
            DifficultyLevel.BEGINNER: 0,
            DifficultyLevel.INTERMEDIATE: 0,
            DifficultyLevel.ADVANCED: 1,
            DifficultyLevel.EXPERT: 2,
        }.get(difficulty, 0)

        if challenge_idx >= len(airport.challenges):
            challenge_idx = len(airport.challenges) - 1

        challenge = airport.challenges[challenge_idx]
        return {
            "name": challenge["name"],
            "description": challenge["description"],
            "difficulty": challenge.get("difficulty", difficulty.value),
            "objectives": challenge.get("objectives", []),
        }

    def _build_objectives(
        self,
        airport: AirportScenario,
        difficulty: DifficultyLevel,
        challenge: Dict
    ) -> List[str]:
        """Build scenario objectives"""
        objectives = [
            "Land all aircraft safely",
            "Maintain proper separation (1000 ft vertical / 2 nm horizontal)",
            "Follow realistic procedures",
        ]

        if challenge:
            objectives.extend(challenge.get("objectives", []))

        if difficulty in [DifficultyLevel.ADVANCED, DifficultyLevel.EXPERT]:
            objectives.append("Maximize landing efficiency")

        if difficulty == DifficultyLevel.EXPERT:
            objectives.append("Achieve 15+ landings per hour")

        return objectives

    def _build_success_criteria(self, difficulty: DifficultyLevel) -> Dict:
        """Build success criteria based on difficulty"""
        return {
            DifficultyLevel.BEGINNER: {
                "min_score": 60,
                "max_violations": 5,
                "min_landings": 1,
            },
            DifficultyLevel.INTERMEDIATE: {
                "min_score": 70,
                "max_violations": 2,
                "min_landings": 3,
            },
            DifficultyLevel.ADVANCED: {
                "min_score": 80,
                "max_violations": 1,
                "min_landings": 5,
            },
            DifficultyLevel.EXPERT: {
                "min_score": 85,
                "max_violations": 0,
                "min_landings": 8,
                "min_landings_per_hour": 15,
            },
        }.get(difficulty, {})

    def _calculate_duration(self, difficulty: DifficultyLevel) -> int:
        """Calculate scenario duration in minutes"""
        return {
            DifficultyLevel.BEGINNER: 10,
            DifficultyLevel.INTERMEDIATE: 20,
            DifficultyLevel.ADVANCED: 30,
            DifficultyLevel.EXPERT: 45,
        }.get(difficulty, 20)

    def build_all_scenarios(self) -> Dict[str, Dict]:
        """Build all scenarios for all major airports"""
        all_scenarios = {}

        for airport_code in [a.value for a in AirportCode]:
            try:
                scenarios = self.build_airport_scenarios(airport_code)
                all_scenarios[airport_code] = scenarios
            except Exception as e:
                print(f"Error building scenarios for {airport_code}: {e}")

        return all_scenarios

    def save_scenarios(self, scenarios: Dict, filepath: str = "scenarios/generated_scenarios.json") -> bool:
        """Save scenarios to JSON file"""
        try:
            with open(filepath, 'w') as f:
                json.dump(scenarios, f, indent=2, default=str)
            print(f"Saved {len(scenarios)} airports to {filepath}")
            return True
        except Exception as e:
            print(f"Error saving scenarios: {e}")
            return False


# For numpy compatibility
import numpy as np


def generate_all_scenarios() -> Dict[str, Dict]:
    """Generate all airport scenarios"""
    builder = ScenarioBuilder()
    scenarios = builder.build_all_scenarios()
    builder.save_scenarios(scenarios)
    return scenarios


if __name__ == '__main__':
    # Generate and save all scenarios
    scenarios = generate_all_scenarios()

    # Display summary
    print("\nScenario Summary:")
    for airport, difficulty_variants in scenarios.items():
        print(f"\n{airport}:")
        for difficulty, scenario in difficulty_variants.items():
            traffic_count = len(scenario.get("traffic", []))
            print(f"  {difficulty}: {traffic_count} aircraft, {scenario.get('duration_minutes')} minutes")
