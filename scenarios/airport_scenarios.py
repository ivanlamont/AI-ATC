"""
Realistic Airport Scenarios for AI-ATC

Includes real runway configurations, procedures, and traffic patterns for major airports.

Airports included:
- SFO (San Francisco International)
- LAX (Los Angeles International)
- JFK (John F. Kennedy International)
- ORD (Chicago O'Hare International)
- ATL (Hartsfield-Jackson Atlanta International)
"""

from dataclasses import dataclass, field
from typing import List, Dict, Tuple
from enum import Enum
import numpy as np


class AirportCode(Enum):
    """Major US airports"""
    SFO = "SFO"  # San Francisco
    LAX = "LAX"  # Los Angeles
    JFK = "JFK"  # New York JFK
    ORD = "ORD"  # Chicago O'Hare
    ATL = "ATL"  # Atlanta


@dataclass
class Runway:
    """Runway configuration"""
    runway_id: str
    heading_deg: float
    length_ft: int
    width_ft: int
    surface: str = "asphalt"
    ils_equipped: bool = True
    faf_distance_nm: float = 6.0
    max_crosswind_kts: float = 15.0


@dataclass
class NavigationFix:
    """Navigation fix (waypoint)"""
    fix_id: str
    position_nm: Tuple[float, float]  # (x, y)
    fix_type: str  # "fix", "vor", "ndb", "runway"
    altitude_restriction_ft: int = 0  # 0 = no restriction


@dataclass
class Procedure:
    """Approach or departure procedure"""
    name: str
    procedure_type: str  # "approach", "departure", "holding"
    runway: str
    fixes: List[NavigationFix]
    altitude_restrictions: List[int]  # Feet at each fix
    speed_restrictions: List[int]  # Knots at each fix
    description: str = ""


@dataclass
class AirportScenario:
    """Complete airport scenario"""
    airport_code: AirportCode
    name: str
    position_nm: Tuple[float, float]
    elevation_ft: int
    runways: List[Runway]
    procedures: List[Procedure]
    typical_traffic: List[Dict] = field(default_factory=list)
    weather_patterns: List[Dict] = field(default_factory=list)
    challenges: List[Dict] = field(default_factory=list)
    voice_pack: Dict = field(default_factory=dict)


# ============================================================================
# SFO - San Francisco International
# Known for: Parallel approaches, marine layer, challenging winds
# ============================================================================

SFO_AIRPORT = AirportScenario(
    airport_code=AirportCode.SFO,
    name="San Francisco International Airport",
    position_nm=(0.0, 0.0),
    elevation_ft=13,
    runways=[
        Runway("28L", 280.0, 11066, 150, ils_equipped=True),
        Runway("28R", 280.0, 10604, 150, ils_equipped=True),
        Runway("01L", 10.0, 10604, 150, ils_equipped=True),
        Runway("01R", 10.0, 11066, 150, ils_equipped=True),
    ],
    procedures=[
        Procedure(
            name="RNAV Approach RWY 28L",
            procedure_type="approach",
            runway="28L",
            fixes=[
                NavigationFix("MARBL", (10.0, 5.0), "fix", 6000),
                NavigationFix("TRONC", (5.0, 3.0), "fix", 3000),
                NavigationFix("MADBE", (2.0, 1.0), "fix", 1500),
            ],
            altitude_restrictions=[6000, 3000, 1500],
            speed_restrictions=[250, 200, 160],
            description="Parallel runway approach RWY 28L via MARBL transition"
        ),
        Procedure(
            name="RNAV Approach RWY 28R",
            procedure_type="approach",
            runway="28R",
            fixes=[
                NavigationFix("MARBL", (10.0, 5.0), "fix", 6000),
                NavigationFix("TRONC", (5.0, 3.0), "fix", 3000),
                NavigationFix("MADBE", (2.0, 1.0), "fix", 1500),
            ],
            altitude_restrictions=[6000, 3000, 1500],
            speed_restrictions=[250, 200, 160],
            description="Parallel runway approach RWY 28R via MARBL transition"
        ),
    ],
    typical_traffic=[
        {
            "aircraft_id": "UAL",
            "aircraft_type": "B737",
            "count": 3,
            "weight": "heavy",
        },
        {
            "aircraft_id": "AAL",
            "aircraft_type": "A320",
            "count": 2,
            "weight": "medium",
        },
        {
            "aircraft_id": "SWR",
            "aircraft_type": "B737",
            "count": 2,
            "weight": "medium",
        },
    ],
    weather_patterns=[
        {
            "name": "Morning Marine Layer",
            "visibility_sm": 2,
            "ceiling_ft": 1200,
            "wind_speed": 8,
            "wind_direction": 270,
            "description": "Typical morning fog condition"
        },
        {
            "name": "Afternoon Sea Breeze",
            "visibility_sm": 8,
            "ceiling_ft": 3000,
            "wind_speed": 12,
            "wind_direction": 290,
            "description": "Clear afternoon with westerly wind"
        },
        {
            "name": "Night Calm",
            "visibility_sm": 10,
            "ceiling_ft": 5000,
            "wind_speed": 4,
            "wind_direction": 270,
            "description": "Clear night conditions"
        },
    ],
    challenges=[
        {
            "name": "Parallel Runway Approach",
            "description": "Manage simultaneous approaches to RWY 28L and 28R with proper spacing",
            "difficulty": "advanced",
            "objectives": [
                "Maintain proper spacing (1000ft vertical or 2nm horizontal)",
                "Sequence two aircraft for parallel approaches",
                "Execute smooth transitions",
            ]
        },
        {
            "name": "Marine Layer Approach",
            "description": "Navigate through marine layer with low visibility and ceiling",
            "difficulty": "intermediate",
            "objectives": [
                "Use instruments for approach",
                "Maintain heading alignment",
                "Manage descent profile in low visibility",
            ]
        },
        {
            "name": "Wind Shift Landing",
            "description": "Handle significant wind shift during approach sequence",
            "difficulty": "advanced",
            "objectives": [
                "Detect wind shift from 270 to 290 degrees",
                "Adjust approach vectors accordingly",
                "Maintain separation during modification",
            ]
        },
    ],
    voice_pack={
        "station_id": "San Francisco Ground",
        "atc_phrases": {
            "welcome": "Welcome to San Francisco Approach",
            "descend": "Descend and maintain three thousand",
            "approach_clearance": "Cleared for parallel approach to two-eight left",
            "landing_clearance": "Cleared to land runway two-eight left, wind two-seven-zero at eight knots",
        }
    }
)


# ============================================================================
# LAX - Los Angeles International
# Known for: Complex runway system, crossing runways, heavy traffic
# ============================================================================

LAX_AIRPORT = AirportScenario(
    airport_code=AirportCode.LAX,
    name="Los Angeles International Airport",
    position_nm=(0.0, 0.0),
    elevation_ft=126,
    runways=[
        Runway("24L", 240.0, 12923, 200, ils_equipped=True),
        Runway("24R", 240.0, 12923, 200, ils_equipped=True),
        Runway("06L", 60.0, 12923, 200, ils_equipped=True),
        Runway("06R", 60.0, 12923, 200, ils_equipped=True),
        Runway("25L", 250.0, 10860, 150, ils_equipped=True),
        Runway("25R", 250.0, 10860, 150, ils_equipped=True),
    ],
    procedures=[
        Procedure(
            name="RNAV Approach RWY 24L",
            procedure_type="approach",
            runway="24L",
            fixes=[
                NavigationFix("SANER", (15.0, 8.0), "fix", 5000),
                NavigationFix("FAJEN", (8.0, 4.0), "fix", 2500),
                NavigationFix("RIDLE", (2.0, 1.0), "fix", 1500),
            ],
            altitude_restrictions=[5000, 2500, 1500],
            speed_restrictions=[250, 200, 150],
            description="Approach to LAX RWY 24L"
        ),
        Procedure(
            name="RNAV Approach RWY 24R",
            procedure_type="approach",
            runway="24R",
            fixes=[
                NavigationFix("SANER", (15.0, 8.0), "fix", 5000),
                NavigationFix("FAJEN", (8.0, 4.0), "fix", 2500),
                NavigationFix("RIDLE", (2.0, 1.0), "fix", 1500),
            ],
            altitude_restrictions=[5000, 2500, 1500],
            speed_restrictions=[250, 200, 150],
            description="Approach to LAX RWY 24R"
        ),
    ],
    typical_traffic=[
        {
            "aircraft_id": "AAL",
            "aircraft_type": "B777",
            "count": 4,
            "weight": "heavy",
        },
        {
            "aircraft_id": "UAL",
            "aircraft_type": "B787",
            "count": 3,
            "weight": "heavy",
        },
        {
            "aircraft_id": "DAL",
            "aircraft_type": "A321",
            "count": 3,
            "weight": "medium",
        },
        {
            "aircraft_id": "SWR",
            "aircraft_type": "B737",
            "count": 2,
            "weight": "medium",
        },
    ],
    weather_patterns=[
        {
            "name": "Santa Ana Winds",
            "visibility_sm": 6,
            "ceiling_ft": 2500,
            "wind_speed": 18,
            "wind_direction": 100,
            "description": "Strong Santa Ana winds from the desert"
        },
        {
            "name": "Marine Inversion",
            "visibility_sm": 4,
            "ceiling_ft": 1500,
            "wind_speed": 10,
            "wind_direction": 240,
            "description": "Marine layer with inversion layer"
        },
        {
            "name": "Clear Southern California",
            "visibility_sm": 10,
            "ceiling_ft": 5000,
            "wind_speed": 8,
            "wind_direction": 250,
            "description": "Typical clear LA weather"
        },
    ],
    challenges=[
        {
            "name": "High Density Traffic",
            "description": "Manage 6+ aircraft in approach sequence with crossing runways",
            "difficulty": "expert",
            "objectives": [
                "Sequence 6 aircraft safely",
                "Maintain proper separation",
                "Manage runway crossing conflicts",
                "Optimize landing throughput",
            ]
        },
        {
            "name": "Santa Ana Wind Challenge",
            "description": "Navigate strong desert winds with variable gusts",
            "difficulty": "advanced",
            "objectives": [
                "Handle 18+ knot winds from unusual direction",
                "Manage wind shear warnings",
                "Execute stable approaches in gusty conditions",
            ]
        },
        {
            "name": "Runway Crossing Management",
            "description": "Manage aircraft crossing active runways during arrivals/departures",
            "difficulty": "advanced",
            "objectives": [
                "Sequence arrivals on 24L/R",
                "Allow departures on 25L/R to cross",
                "Maintain safety throughout sequence",
            ]
        },
    ],
    voice_pack={
        "station_id": "Los Angeles Approach",
        "atc_phrases": {
            "welcome": "Welcome to Los Angeles Approach",
            "descend": "Descend and maintain two thousand five hundred",
            "approach_clearance": "Cleared for approach runway two-four left",
            "landing_clearance": "Cleared to land runway two-four left, wind two-four-zero at eight knots",
        }
    }
)


# ============================================================================
# JFK - John F. Kennedy International
# Known for: Complex procedures, heavy traffic, cold weather
# ============================================================================

JFK_AIRPORT = AirportScenario(
    airport_code=AirportCode.JFK,
    name="John F. Kennedy International Airport",
    position_nm=(0.0, 0.0),
    elevation_ft=13,
    runways=[
        Runway("04L", 40.0, 14511, 200, ils_equipped=True),
        Runway("04R", 40.0, 13000, 200, ils_equipped=True),
        Runway("22L", 220.0, 14000, 200, ils_equipped=True),
        Runway("22R", 220.0, 14511, 200, ils_equipped=True),
        Runway("13L", 130.0, 8550, 150, ils_equipped=False),
        Runway("13R", 130.0, 8550, 150, ils_equipped=False),
    ],
    procedures=[
        Procedure(
            name="STAR Arrival CANOE TWO",
            procedure_type="arrival",
            runway="04L",
            fixes=[
                NavigationFix("CANOE", (25.0, 15.0), "fix", 4000),
                NavigationFix("DOVER", (15.0, 8.0), "fix", 2500),
                NavigationFix("LNSKY", (5.0, 2.0), "fix", 1500),
            ],
            altitude_restrictions=[4000, 2500, 1500],
            speed_restrictions=[250, 200, 160],
            description="CANOE TWO departure to JFK RWY 04"
        ),
    ],
    typical_traffic=[
        {
            "aircraft_id": "DAL",
            "aircraft_type": "B777",
            "count": 4,
            "weight": "heavy",
        },
        {
            "aircraft_id": "BAW",
            "aircraft_type": "B777",
            "count": 3,
            "weight": "heavy",
        },
        {
            "aircraft_id": "UAL",
            "aircraft_type": "B767",
            "count": 3,
            "weight": "heavy",
        },
        {
            "aircraft_id": "AAL",
            "aircraft_type": "A321",
            "count": 2,
            "weight": "medium",
        },
    ],
    weather_patterns=[
        {
            "name": "Winter Conditions",
            "visibility_sm": 2,
            "ceiling_ft": 800,
            "wind_speed": 15,
            "wind_direction": 330,
            "description": "Winter snow and low visibility"
        },
        {
            "name": "Spring Storms",
            "visibility_sm": 3,
            "ceiling_ft": 1200,
            "wind_speed": 20,
            "wind_direction": 180,
            "description": "Spring thunderstorms"
        },
        {
            "name": "Clear Summer",
            "visibility_sm": 10,
            "ceiling_ft": 5000,
            "wind_speed": 10,
            "wind_direction": 220,
            "description": "Clear summer day"
        },
    ],
    challenges=[
        {
            "name": "Winter Landing Challenge",
            "description": "Land heavy aircraft in winter conditions with low visibility",
            "difficulty": "expert",
            "objectives": [
                "Manage low ceiling approach",
                "Handle strong northerly winds",
                "Execute stable landing",
                "Avoid go-arounds",
            ]
        },
        {
            "name": "Complex Arrival Procedures",
            "description": "Follow STAR procedures with multiple waypoints and restrictions",
            "difficulty": "advanced",
            "objectives": [
                "Follow CANOE TWO procedure exactly",
                "Maintain altitude restrictions",
                "Execute speed reductions at each fix",
            ]
        },
    ],
    voice_pack={
        "station_id": "New York Approach",
        "atc_phrases": {
            "welcome": "Welcome to New York Approach",
            "descend": "Descend and maintain three thousand",
            "approach_clearance": "Cleared for approach runway zero-four left",
            "landing_clearance": "Cleared to land runway zero-four left, wind three-three-zero at twelve knots",
        }
    }
)


# ============================================================================
# ORD - Chicago O'Hare International
# Known for: Challenging weather, complex traffic, frequent delays
# ============================================================================

ORD_AIRPORT = AirportScenario(
    airport_code=AirportCode.ORD,
    name="Chicago O'Hare International Airport",
    position_nm=(0.0, 0.0),
    elevation_ft=682,
    runways=[
        Runway("28C", 280.0, 13000, 200, ils_equipped=True),
        Runway("28L", 280.0, 12000, 200, ils_equipped=True),
        Runway("28R", 280.0, 13000, 200, ils_equipped=True),
        Runway("10L", 100.0, 11000, 150, ils_equipped=True),
        Runway("10R", 100.0, 13000, 150, ils_equipped=True),
        Runway("10C", 100.0, 13000, 150, ils_equipped=True),
    ],
    procedures=[
        Procedure(
            name="RNAV Approach RWY 28L",
            procedure_type="approach",
            runway="28L",
            fixes=[
                NavigationFix("WAUKEE", (20.0, 10.0), "fix", 4000),
                NavigationFix("ARLON", (10.0, 5.0), "fix", 2000),
                NavigationFix("MIDWAY", (2.0, 1.0), "fix", 1000),
            ],
            altitude_restrictions=[4000, 2000, 1000],
            speed_restrictions=[250, 180, 140],
            description="Approach to ORD RWY 28"
        ),
    ],
    typical_traffic=[
        {
            "aircraft_id": "AAL",
            "aircraft_type": "A320",
            "count": 4,
            "weight": "medium",
        },
        {
            "aircraft_id": "UAL",
            "aircraft_type": "B737",
            "count": 4,
            "weight": "medium",
        },
        {
            "aircraft_id": "DAL",
            "aircraft_type": "B737",
            "count": 3,
            "weight": "medium",
        },
        {
            "aircraft_id": "SWR",
            "aircraft_type": "B737",
            "count": 2,
            "weight": "medium",
        },
    ],
    weather_patterns=[
        {
            "name": "Severe Thunderstorms",
            "visibility_sm": 1,
            "ceiling_ft": 500,
            "wind_speed": 25,
            "wind_direction": 270,
            "description": "Summer thunderstorms with strong winds"
        },
        {
            "name": "Lake Effect Snow",
            "visibility_sm": 2,
            "ceiling_ft": 800,
            "wind_speed": 18,
            "wind_direction": 320,
            "description": "Winter lake effect snow from Lake Michigan"
        },
        {
            "name": "Clear Spring",
            "visibility_sm": 10,
            "ceiling_ft": 6000,
            "wind_speed": 8,
            "wind_direction": 180,
            "description": "Clear spring conditions"
        },
    ],
    challenges=[
        {
            "name": "Severe Weather Management",
            "description": "Handle thunderstorms with go-arounds and sequence changes",
            "difficulty": "expert",
            "objectives": [
                "Issue go-arounds due to weather",
                "Re-sequence aircraft",
                "Manage multiple approaches safely",
            ]
        },
        {
            "name": "Triple Runway Approach",
            "description": "Manage approaches to three parallel runways simultaneously",
            "difficulty": "expert",
            "objectives": [
                "Sequence 6 aircraft to three runways",
                "Maintain proper spacing",
                "Optimize landing rate",
            ]
        },
    ],
    voice_pack={
        "station_id": "Chicago Approach",
        "atc_phrases": {
            "welcome": "Welcome to Chicago Approach",
            "descend": "Descend and maintain two thousand",
            "approach_clearance": "Cleared for approach runway two-eight left",
            "landing_clearance": "Cleared to land runway two-eight left, wind two-eight-zero at ten knots",
        }
    }
)


# ============================================================================
# ATL - Hartsfield-Jackson Atlanta International
# Known for: World's busiest airport, high traffic volume
# ============================================================================

ATL_AIRPORT = AirportScenario(
    airport_code=AirportCode.ATL,
    name="Hartsfield-Jackson Atlanta International Airport",
    position_nm=(0.0, 0.0),
    elevation_ft=1026,
    runways=[
        Runway("27L", 270.0, 13000, 150, ils_equipped=True),
        Runway("27R", 270.0, 13000, 150, ils_equipped=True),
        Runway("09L", 90.0, 12000, 150, ils_equipped=True),
        Runway("09R", 90.0, 12000, 150, ils_equipped=True),
        Runway("27C", 270.0, 13000, 150, ils_equipped=True),
        Runway("09C", 90.0, 13000, 150, ils_equipped=True),
    ],
    procedures=[
        Procedure(
            name="RNAV Approach RWY 27L",
            procedure_type="approach",
            runway="27L",
            fixes=[
                NavigationFix("DOBBS", (20.0, 12.0), "fix", 4000),
                NavigationFix("KENUP", (10.0, 6.0), "fix", 2000),
                NavigationFix("PEACH", (2.0, 1.0), "fix", 1000),
            ],
            altitude_restrictions=[4000, 2000, 1000],
            speed_restrictions=[250, 180, 140],
            description="Approach to ATL RWY 27"
        ),
    ],
    typical_traffic=[
        {
            "aircraft_id": "DAL",
            "aircraft_type": "A321",
            "count": 6,
            "weight": "medium",
        },
        {
            "aircraft_id": "AAL",
            "aircraft_type": "A320",
            "count": 5,
            "weight": "medium",
        },
        {
            "aircraft_id": "UAL",
            "aircraft_type": "B737",
            "count": 4,
            "weight": "medium",
        },
        {
            "aircraft_id": "SWR",
            "aircraft_type": "B737",
            "count": 3,
            "weight": "medium",
        },
    ],
    weather_patterns=[
        {
            "name": "Summer Heat",
            "visibility_sm": 8,
            "ceiling_ft": 3000,
            "wind_speed": 12,
            "wind_direction": 230,
            "description": "Hot summer day with afternoon haze"
        },
        {
            "name": "Spring Severe Weather",
            "visibility_sm": 3,
            "ceiling_ft": 1500,
            "wind_speed": 22,
            "wind_direction": 190,
            "description": "Spring severe weather season"
        },
        {
            "name": "Fall Clear",
            "visibility_sm": 10,
            "ceiling_ft": 5000,
            "wind_speed": 8,
            "wind_direction": 270,
            "description": "Clear fall conditions"
        },
    ],
    challenges=[
        {
            "name": "Ultra High Density",
            "description": "Manage world's busiest airport with 8+ simultaneous aircraft",
            "difficulty": "expert",
            "objectives": [
                "Land 8 aircraft safely in sequence",
                "Maintain perfect separation",
                "Maximize landing efficiency (15+ per hour)",
                "Handle continuous demand",
            ]
        },
        {
            "name": "Runway Configuration Change",
            "description": "Change from 27s to 09s during approach sequence",
            "difficulty": "expert",
            "objectives": [
                "Detect runway change need",
                "Issue go-arounds",
                "Re-sequence aircraft",
                "Execute new approaches",
            ]
        },
        {
            "name": "Severe Weather Avoidance",
            "description": "Route aircraft around spring thunderstorms",
            "difficulty": "advanced",
            "objectives": [
                "Identify storm cells",
                "Vector around weather",
                "Maintain efficient routing",
                "Ensure all aircraft land safely",
            ]
        },
    ],
    voice_pack={
        "station_id": "Atlanta Approach",
        "atc_phrases": {
            "welcome": "Welcome to Atlanta Approach",
            "descend": "Descend and maintain two thousand",
            "approach_clearance": "Cleared for approach runway two-seven left",
            "landing_clearance": "Cleared to land runway two-seven left, wind two-seven-zero at eight knots",
        }
    }
)


# Dictionary of all major airports
MAJOR_AIRPORTS = {
    AirportCode.SFO: SFO_AIRPORT,
    AirportCode.LAX: LAX_AIRPORT,
    AirportCode.JFK: JFK_AIRPORT,
    AirportCode.ORD: ORD_AIRPORT,
    AirportCode.ATL: ATL_AIRPORT,
}


def get_airport_scenario(airport_code: str) -> AirportScenario:
    """Get scenario for specified airport"""
    try:
        code = AirportCode(airport_code)
        return MAJOR_AIRPORTS[code]
    except KeyError:
        raise ValueError(f"Unknown airport code: {airport_code}")
    except ValueError:
        raise ValueError(f"Invalid airport code: {airport_code}. Must be one of: SFO, LAX, JFK, ORD, ATL")


def list_available_airports() -> List[str]:
    """List all available airport scenarios"""
    return [airport.value for airport in AirportCode]


if __name__ == '__main__':
    # Display available airports
    print("Available Airport Scenarios:")
    for airport_code in list_available_airports():
        scenario = get_airport_scenario(airport_code)
        print(f"\n{airport_code}: {scenario.name}")
        print(f"  Elevation: {scenario.elevation_ft} ft")
        print(f"  Runways: {', '.join(r.runway_id for r in scenario.runways)}")
        print(f"  Challenges: {len(scenario.challenges)}")
        print(f"  Weather patterns: {len(scenario.weather_patterns)}")
