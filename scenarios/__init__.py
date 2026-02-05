"""
AI-ATC Scenario Module

Provides realistic airport scenarios for training and gameplay.
"""

from scenarios.airport_scenarios import (
    AirportCode,
    AirportScenario,
    Runway,
    NavigationFix,
    Procedure,
    get_airport_scenario,
    list_available_airports,
    MAJOR_AIRPORTS,
)

from scenarios.scenario_builder import (
    ScenarioBuilder,
    DifficultyLevel,
    ScenarioDifficulty,
    generate_all_scenarios,
)

__all__ = [
    "AirportCode",
    "AirportScenario",
    "Runway",
    "NavigationFix",
    "Procedure",
    "get_airport_scenario",
    "list_available_airports",
    "MAJOR_AIRPORTS",
    "ScenarioBuilder",
    "DifficultyLevel",
    "ScenarioDifficulty",
    "generate_all_scenarios",
]
