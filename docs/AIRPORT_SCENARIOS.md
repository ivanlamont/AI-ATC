# ✈️ Realistic Airport Scenarios

AI-ATC includes detailed, realistic scenarios for five major US airports. Each airport features authentic runway configurations, procedures, traffic patterns, and unique challenges.

## Overview

| Airport | Location | Specialty | Challenges |
|---------|----------|-----------|-----------|
| **SFO** | San Francisco, CA | Parallel approaches, marine layer | Dual parallel, fog navigation |
| **LAX** | Los Angeles, CA | Complex runway system, crossings | High density, crossing runways |
| **JFK** | New York, NY | Complex procedures, international | Winter ops, advanced procedures |
| **ORD** | Chicago, IL | Heavy traffic, weather | Thunderstorms, multiple runways |
| **ATL** | Atlanta, GA | World's busiest, high density | Ultra-high density, efficiency |

---

## SFO - San Francisco International

### Airport Overview
- **Elevation**: 13 ft MSL
- **Runways**: 4 parallel runways (28L/R, 01L/R)
- **Location**: ~8 miles south of downtown San Francisco
- **Signature Challenge**: Parallel runway operations in marine layer

### Runway Configuration

```
RWY 28L: 11,066 ft × 150 ft (ILS)  ← Primary westbound
RWY 28R: 10,604 ft × 150 ft (ILS)
RWY 01L: 10,604 ft × 150 ft (ILS)  ← Primary eastbound
RWY 01R: 11,066 ft × 150 ft (ILS)
```

### Approach Procedures

**RNAV Approach RWY 28L/R via MARBL**
- Entry: MARBL fix at 6,000 ft
- Intermediate: TRONC fix at 3,000 ft
- Final: MADBE fix at 1,500 ft
- Landing distance: 6 NM final approach

### Weather Patterns

| Pattern | Visibility | Ceiling | Wind | Time |
|---------|-----------|---------|------|------|
| Morning Marine Layer | 2 SM | 1,200 ft | 270° @ 8kt | Dawn-9am |
| Afternoon Sea Breeze | 8 SM | 3,000 ft | 290° @ 12kt | 9am-6pm |
| Night Calm | 10 SM | 5,000 ft | 270° @ 4kt | 6pm-dawn |

### Typical Traffic

- **United**: 3 B737s
- **American**: 2 A320s
- **Southwest**: 2 B737s

### Challenges

#### Beginner: Basic Approach
- Single aircraft approach
- Clear conditions
- Straight descent profile

#### Intermediate: Marine Layer
- 2 aircraft approaching
- Low visibility, marine layer
- Challenge: Navigate through fog layer

#### Advanced: Parallel Approaches
- 2-3 aircraft for parallel runways
- Maintain 1,000 ft vertical or 2 nm horizontal separation
- Challenge: Coordinate simultaneous approaches to 28L and 28R

#### Expert: Parallel + Weather + Wind Shift
- 4+ aircraft
- Marine layer with wind shift (270° → 290°)
- Challenge: Handle configuration changes mid-sequence

---

## LAX - Los Angeles International

### Airport Overview
- **Elevation**: 126 ft MSL
- **Runways**: 6 runways (24L/R, 06L/R, 25L/R)
- **Location**: ~16 miles southwest of downtown LA
- **Signature Challenge**: Complex runway system with crossings

### Runway Configuration

```
Parallel Pair 1:
  RWY 24L: 12,923 ft × 200 ft (ILS)
  RWY 24R: 12,923 ft × 200 ft (ILS)

Parallel Pair 2:
  RWY 06L: 12,923 ft × 200 ft (ILS)
  RWY 06R: 12,923 ft × 200 ft (ILS)

Crossing Runways:
  RWY 25L: 10,860 ft × 150 ft (ILS)
  RWY 25R: 10,860 ft × 150 ft (ILS)
```

### Approach Procedures

**RNAV Approach RWY 24L/R via SANER**
- Entry: SANER fix at 5,000 ft
- Intermediate: FAJEN fix at 2,500 ft
- Final: RIDLE fix at 1,500 ft

### Weather Patterns

| Pattern | Visibility | Ceiling | Wind | Cause |
|---------|-----------|---------|------|-------|
| Santa Ana Winds | 6 SM | 2,500 ft | 100° @ 18kt | Desert winds |
| Marine Inversion | 4 SM | 1,500 ft | 240° @ 10kt | Marine layer |
| Clear SoCal | 10 SM | 5,000 ft | 250° @ 8kt | Typical conditions |

### Typical Traffic

- **American**: 4 B777s (heavy)
- **United**: 3 B787s (heavy)
- **Delta**: 3 A321s (medium)
- **Southwest**: 2 B737s (medium)

### Challenges

#### Intermediate: High Density
- 4+ aircraft approaching simultaneously
- Multiple runways available
- Challenge: Sequence efficiently to maximize landings

#### Advanced: Runway Crossings
- Arrivals on 24L/R, Departures on 25L/R
- Manage traffic crossing active runways
- Challenge: Separate arrival and departure flows

#### Expert: Full Complexity
- 6+ aircraft
- Runway crossings
- Santa Ana winds (18 kt from 100°)
- Challenge: Perfect sequencing with all complexities

---

## JFK - John F. Kennedy International

### Airport Overview
- **Elevation**: 13 ft MSL
- **Runways**: 6 runways (04L/R, 22L/R, 13L/R)
- **Location**: ~15 miles east of Manhattan
- **Signature Challenge**: Complex international procedures, winter weather

### Runway Configuration

```
Primary Pair:
  RWY 04L: 14,511 ft × 200 ft (ILS)
  RWY 04R: 13,000 ft × 200 ft (ILS)

Secondary Pair:
  RWY 22L: 14,000 ft × 200 ft (ILS)
  RWY 22R: 14,511 ft × 200 ft (ILS)

Crosswind Runways:
  RWY 13L: 8,550 ft × 150 ft (non-ILS)
  RWY 13R: 8,550 ft × 150 ft (non-ILS)
```

### Standard Terminal Arrival Routes (STARs)

**CANOE TWO Arrival**
- Entry: CANOE fix at 4,000 ft
- Intermediate: DOVER fix at 2,500 ft
- Final: LNSKY fix at 1,500 ft
- Typical for eastbound arrivals

### Weather Patterns

| Pattern | Visibility | Ceiling | Wind | Season |
|---------|-----------|---------|------|--------|
| Winter Conditions | 2 SM | 800 ft | 330° @ 15kt | Nov-Mar |
| Spring Storms | 3 SM | 1,200 ft | 180° @ 20kt | Apr-May |
| Summer Haze | 6 SM | 2,500 ft | 240° @ 12kt | Jun-Aug |
| Fall Clear | 10 SM | 5,000 ft | 220° @ 8kt | Sep-Oct |

### Typical Traffic

International and Domestic Mix:
- **Delta**: 4 B777s (heavy)
- **British Airways**: 3 B777s (heavy)
- **United**: 3 B767s (heavy)
- **American**: 2 A321s (medium)

### Challenges

#### Intermediate: Advanced Procedures
- Follow STAR procedures precisely
- Maintain altitude and speed restrictions
- Challenge: Complex approach with multiple fixes

#### Advanced: Winter Operations
- Low visibility, low ceiling
- Strong northerly winds
- Heavy aircraft handling
- Challenge: Land safely in winter conditions

#### Expert: Winter + Complex + Heavy Traffic
- 4+ aircraft
- Winter weather (ceiling 800 ft)
- Heavy aircraft (B777s)
- Challenge: Perfect STAR procedure execution with all weather/traffic

---

## ORD - Chicago O'Hare International

### Airport Overview
- **Elevation**: 682 ft MSL
- **Runways**: 6 runways in two groups (10s and 28s)
- **Location**: ~18 miles northwest of downtown Chicago
- **Signature Challenge**: Severe weather management, high density

### Runway Configuration

```
RWY 28 Group (Westbound):
  RWY 28C: 13,000 ft × 200 ft (ILS)  ← Center
  RWY 28L: 12,000 ft × 200 ft (ILS)  ← Left
  RWY 28R: 13,000 ft × 200 ft (ILS)  ← Right

RWY 10 Group (Eastbound):
  RWY 10C: 13,000 ft × 150 ft (ILS)  ← Center
  RWY 10L: 11,000 ft × 150 ft (ILS)  ← Left
  RWY 10R: 13,000 ft × 150 ft (ILS)  ← Right
```

### Approach Procedures

**RNAV Approach RWY 28L via WAUKEE**
- Entry: WAUKEE fix at 4,000 ft
- Intermediate: ARLON fix at 2,000 ft
- Final: MIDWAY fix at 1,000 ft

### Weather Patterns

| Pattern | Visibility | Ceiling | Wind | Challenge |
|---------|-----------|---------|------|-----------|
| Thunderstorms | 1 SM | 500 ft | 270° @ 25kt | Severe |
| Lake Effect Snow | 2 SM | 800 ft | 320° @ 18kt | Winter |
| Spring Clear | 10 SM | 6,000 ft | 180° @ 8kt | Ideal |

### Typical Traffic

Mostly domestic carriers:
- **American**: 4 A320s
- **United**: 4 B737s
- **Delta**: 3 B737s
- **Southwest**: 2 B737s

### Challenges

#### Intermediate: Triple Runway
- Approach 3+ aircraft to different runways (28C, 28L, 28R)
- Challenge: Proper sequencing to three parallel runways

#### Advanced: Severe Weather
- Thunderstorms with low visibility/ceiling
- Strong winds and wind shear
- Go-around requirements
- Challenge: Handle weather-induced changes and re-sequencing

#### Expert: Storm + Density + Configuration Change
- 6+ aircraft
- Active thunderstorms
- Potential runway change
- Challenge: Complete control under extreme conditions

---

## ATL - Hartsfield-Jackson Atlanta International

### Airport Overview
- **Elevation**: 1,026 ft MSL
- **Runways**: 6 runways (parallel pairs on both headings)
- **Location**: ~9 miles south of downtown Atlanta
- **Signature Challenge**: World's busiest airport, extreme density

### Runway Configuration

```
RWY 27 Group (Westbound - Primary):
  RWY 27L: 13,000 ft × 150 ft (ILS)
  RWY 27C: 13,000 ft × 150 ft (ILS)
  RWY 27R: 13,000 ft × 150 ft (ILS)

RWY 09 Group (Eastbound - Alternate):
  RWY 09L: 12,000 ft × 150 ft (ILS)
  RWY 09C: 13,000 ft × 150 ft (ILS)
  RWY 09R: 12,000 ft × 150 ft (ILS)
```

### Approach Procedures

**RNAV Approach RWY 27L via DOBBS**
- Entry: DOBBS fix at 4,000 ft
- Intermediate: KENUP fix at 2,000 ft
- Final: PEACH fix at 1,000 ft

### Weather Patterns

| Pattern | Visibility | Ceiling | Wind | Impact |
|---------|-----------|---------|------|--------|
| Summer Heat | 8 SM | 3,000 ft | 230° @ 12kt | Haze layer |
| Spring Severe | 3 SM | 1,500 ft | 190° @ 22kt | Thunderstorms |
| Fall Clear | 10 SM | 5,000 ft | 270° @ 8kt | Ideal |

### Typical Traffic

Massive traffic volume:
- **Delta**: 6 A321s (hub)
- **American**: 5 A320s
- **United**: 4 B737s
- **Southwest**: 3 B737s

### Challenges

#### Intermediate: High Density
- 4+ aircraft on approach
- Multiple runway options
- Challenge: Efficient sequencing

#### Advanced: Runway Configuration Change
- Operating on RWY 27s
- Wind shift requires change to RWY 09s
- Re-sequence 4+ aircraft
- Challenge: Execute major config change mid-scenario

#### Expert: Ultra-High Density
- 8+ simultaneous aircraft
- Complex sequencing to three parallel runways
- Maintain 15+ landing rate (per hour equivalent)
- Spring severe weather
- Challenge: Perfect execution at world's busiest airport

---

## Scenario Difficulty Progression

### Beginner (Single Airport)
- 1 aircraft
- Clear weather
- No special challenges
- Estimated time: 10 minutes
- Learning objectives: Master basic procedures

### Intermediate (Same Airport, 2x Difficulty)
- 3 aircraft
- Variable weather
- One procedural challenge
- Estimated time: 20 minutes
- Learning objectives: Manage multiple aircraft

### Advanced (Same Airport, 3x Difficulty)
- 5 aircraft
- Challenging weather
- Multiple challenges combined
- Estimated time: 30 minutes
- Learning objectives: Advanced sequencing and weather handling

### Expert (Same Airport, Maximum Difficulty)
- 8+ aircraft
- Severe weather
- All challenges active
- Estimated time: 45 minutes
- Learning objectives: Master all airport procedures

---

## Training Progression

Recommended learning path:

```
Week 1: SFO Basics
├── Beginner: Simple approach (marine layer study)
├── Beginner: Two sequential (spacing practice)
└── Intermediate: Parallel approaches (main challenge)

Week 2: LAX Operations
├── Intermediate: High density (4+ aircraft)
├── Advanced: Runway crossings (traffic separation)
└── Advanced: Santa Ana winds (wind handling)

Week 3: JFK Procedures
├── Intermediate: STAR procedures (advanced nav)
├── Advanced: Winter operations (weather handling)
└── Advanced: Heavy aircraft (B777 management)

Week 4: ORD Weather
├── Intermediate: Triple parallel (3-runway ops)
├── Advanced: Severe storms (weather challenges)
└── Expert: Complete scenario (all skills)

Week 5: ATL Density
├── Advanced: Density operations (6+ aircraft)
├── Expert: Configuration changes (dynamic ops)
└── Expert: Ultra-high density (world-class ops)
```

---

## Tips by Airport

### SFO
- Learn parallel approach timing
- Watch for wind shifts (270° → 290°)
- Navigate marine layer with instruments

### LAX
- Understand runway crossing procedures
- Watch for Santa Ana winds
- Manage high traffic volume efficiently

### JFK
- Master STAR procedures exactly
- Handle winter weather conservatively
- Work with heavy aircraft (larger turning radius)

### ORD
- Prepare for thunderstorms
- Use all three parallel runways
- Watch for wind shear alerts

### ATL
- Maximize landing throughput
- Handle world-record traffic density
- Practice rapid re-sequencing

---

## Real-World Details

All scenarios include:
- **Accurate Runway Specs**: Real dimensions, ILS equip
- **Realistic Procedures**: Based on actual STARs/IAPs
- **Weather Patterns**: Typical seasonal patterns
- **Traffic Mix**: Realistic airline mix for each airport
- **ATC Phraseology**: Region-appropriate terminology
- **Signature Challenges**: Based on known airport characteristics

---

## Next Steps

1. **Start with SFO Beginner**: Learn basics in marine layer
2. **Progress through Difficulties**: Each airport has 4 variants
3. **Complete All Airports**: Master all 5 major hubs
4. **Focus on Your Weakness**: Revisit challenging scenarios
5. **Compete on Leaderboards**: Compare scores with other players

---

## See Also

- [Getting Started](GETTING_STARTED.md) — New player guide
- [Scenarios](SCENARIOS.md) — All scenario types
- [Best Practices](BEST_PRACTICES.md) — Expert techniques
- [Keyboard Shortcuts](KEYBOARD_SHORTCUTS.md) — Control reference
