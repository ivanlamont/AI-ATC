# 📊 Scoring System

AI-ATC uses a comprehensive scoring system that rewards safe, efficient air traffic control. This guide explains how scores are calculated and how to maximize your performance.

## Scoring Overview

Your score is calculated from multiple factors:

```
Total Score = Safety Score + Efficiency Score + Bonus Score - Penalties
```

Maximum possible score: **100 points**

### Score Components

1. **Safety Score** (40 points)
2. **Efficiency Score** (40 points)
3. **Bonus Score** (variable)
4. **Penalties** (cumulative)

## Safety Score (40 points)

### Separation Violations (Critical)

**Violation Types**:
- **Horizontal**: Aircraft < 2 nautical miles apart at same altitude
- **Vertical**: Aircraft < 1,000 feet apart
- **Wake Turbulence**: Heavy aircraft too close to lighter aircraft

**Penalties**:
- **Each violation**: -5 points
- **Multiple violations**: -10 points for 3+ violations
- **Severe violation**: -15 points (aircraft collision)

**Examples**:
```
Safe: AAL456 at FL250, UAL789 at FL240 (1,000 ft apart) = 0 penalty
Violation: AAL456 at FL250, UAL789 at FL249 (100 ft apart) = -5 points
```

### Altitude Limit Violations

**Violation Types**:
- **Exceeding ceiling**: Aircraft above maximum altitude
- **Descending below minimum**: Aircraft below safe altitude
- **Class B airspace**: Entering restricted zones

**Penalties**:
- **Each violation**: -3 points
- **Sustained violation**: -1 point per 10 seconds
- **Airspace bust**: -10 points

**Examples**:
```
Safe: Descent from 5,000 to 3,000 feet = 0 penalty
Violation: Aircraft descends to 400 feet (below 500 ft minimum) = -3 points
```

### Speed Violations

**Violation Types**:
- **Exceeding max speed**: Aircraft faster than aircraft limit
- **Below minimum speed**: Aircraft slower than safe speed (especially approach)
- **Speed limit zones**: Specific areas with restricted speeds

**Penalties**:
- **Each violation**: -2 points
- **Sustained violation**: -1 point per 10 seconds
- **Critical (stall risk)**: -5 points

**Examples**:
```
Safe: Commercial jet at 180 knots on approach = 0 penalty
Violation: Attempting 500 knots in Class B airspace = -2 points
```

### Approach/Landing Violations

**Violation Types**:
- **Unstable approach**: Erratic altitude/speed during landing
- **Missed alignment**: Aircraft not properly aligned on final
- **Crosswind exceeded**: Landing attempted with excessive crosswind
- **Collision with terrain**: Aircraft hits ground before runway

**Penalties**:
- **Each violation**: -5 points
- **Go-around required**: -3 points (safety measure)
- **Missed runway**: -10 points
- **Ground collision**: -25 points (scenario failure)

### Landing Success Bonus

**Landing Bonus**:
- **Successful landing**: +5 points
- **Soft landing** (excellent): +10 points
- **First-time landing** (no go-around): +2 bonus points

**Requirements for Soft Landing**:
- Descent rate < 300 feet/minute
- Alignment within 5 degrees
- Speed within 10 knots of target
- Wind within limits

## Efficiency Score (40 points)

### Landing Rate

**Definition**: Aircraft landed per hour of simulation

**Scoring**:
- **5 aircraft/hour**: 20 points
- **10 aircraft/hour**: 30 points
- **15 aircraft/hour**: 40 points
- **20+ aircraft/hour**: 50 points (maximum, expert only)

**Calculation**:
```
Landing Rate = (Total Aircraft Landed / Simulation Duration) × 60
Efficiency Points = min(Landing Rate / 0.5, 40)
```

**Examples**:
```
3 aircraft in 15 minutes = 12 aircraft/hour = 30 points
5 aircraft in 20 minutes = 15 aircraft/hour = 40 points
```

### Vector Quality

**Definition**: How efficiently you directed aircraft to landing

**Factors**:
- **Heading changes**: Fewer unnecessary turns = higher score
- **Speed adjustments**: Smooth speed profiles = higher score
- **Altitude transitions**: Smooth descents = higher score
- **Instruction clarity**: Clear commands = higher score

**Scoring**:
- **0-5 extra commands**: 10 points (excellent)
- **6-10 extra commands**: 7 points (good)
- **11-15 extra commands**: 4 points (average)
- **16+ extra commands**: 1 point (inefficient)

**Optimal Sequence** (minimal commands):
```
1. Vector to approach fix
2. Descend to 3,000
3. Turn for alignment
4. Descend to 1,500
5. Cleared to land
= 5 commands (baseline)
```

### Time to Landing

**Definition**: Time from aircraft first appearance to landing

**Scoring**:
- **Under 10 minutes**: 10 points
- **10-15 minutes**: 8 points
- **15-20 minutes**: 5 points
- **20+ minutes**: 2 points

**Examples**:
```
Single aircraft approaching from 15 nm:
- 8 minutes to land = 10 points
- 12 minutes to land = 8 points
```

### Workload Management

**Definition**: How well you managed multiple aircraft simultaneously

**Scoring**:
- **1 aircraft**: 2 points (baseline)
- **2 aircraft**: 4 points
- **3 aircraft**: 6 points
- **4+ aircraft**: 8 points (maximum)

**Bonuses**:
- **Perfect coordination**: +3 points
- **Zero workload errors**: +2 points
- **Smooth handoffs**: +2 points

### Fuel Economy

**Definition**: Minimizing unnecessary turns and altitude changes

**Scoring**:
- **Optimal route** (direct approach): +5 points
- **Minor deviations**: +3 points
- **Several deviations**: +1 point
- **Excessive vectors**: 0 points

**Calculation**:
```
Distance Flown vs Direct Distance
Ratio = Actual / Direct
Points = max(5 * (2 - Ratio), 0)
```

## Bonus Points

### Challenge Bonuses

**Weather Challenge**:
- Complete scenario in severe weather: +10 points
- Complete with wind changes: +5 points

**Traffic Challenge**:
- Land 5 aircraft with zero violations: +10 points
- Perfect separation throughout: +5 points

**Complexity Challenge**:
- Mixed VFR/IFR operations: +5 points
- High-density traffic: +10 points

**Emergency Challenge**:
- Handle emergency and land successfully: +15 points
- Handle multiple emergencies: +25 points

### Time Bonuses

**Speed Completion**:
- Complete 30% faster than average: +5 points
- Complete 50% faster than average: +10 points

**Peak Hours**:
- Complete during simulated peak hours: +3 points
- Complete multiple consecutive peak hours: +10 points

## Penalties

### Command Errors

**Invalid Commands**: -1 point each
**Repeated errors**: -2 points after 3 errors

**Examples**:
```
- Typo in callsign: -1 point
- Invalid altitude: -1 point
- Conflicting commands: -2 points
```

### Safety Violations (Detailed)

| Violation | Points |
|-----------|--------|
| Separation violation | -5 |
| Altitude bust | -3 |
| Speed violation | -2 |
| Alignment failure | -3 |
| Go-around required | -3 |
| Crosswind exceeded | -5 |
| Collision with terrain | -25 |

### Operational Penalties

**Runway Change Penalties**:
- Premature change: -5 points
- Aircraft disruption: -3 points per aircraft affected

**Configuration Penalties**:
- Invalid configuration: -10 points
- Unsafe configuration: -5 points

## Scoring Examples

### Example 1: Perfect Single Approach

**Scenario**: Basic Single Approach (1 aircraft, 10 minutes)

**Breakdown**:
- Safety: 40 points (0 violations)
  - Separation: 10 points
  - Altitude: 10 points
  - Speed: 10 points
  - Soft landing: 10 points
- Efficiency: 30 points
  - Landing rate: 6 aircraft/hour = 20 points
  - Vector quality: Excellent (5 commands) = 10 points
- Time bonus: 10 points (under 10 minutes)
- Workload: 2 points (single aircraft)

**Total**: 92 points ✓

### Example 2: Multi-Aircraft Scenario

**Scenario**: Three-Aircraft Staggered (3 aircraft, 18 minutes)

**Breakdown**:
- Safety: 35 points
  - 1 minor separation warning (no violation): 0 points
  - Altitudes perfect: 10 points
  - Speeds within limits: 10 points
  - All landings successful: 15 points
- Efficiency: 35 points
  - Landing rate: 10 aircraft/hour = 30 points
  - Vector quality: Good (8 extra commands) = 7 points
  - Workload management: 6 points (3 aircraft)
  - Fuel economy: 3 points
- Weather bonus: 5 points (variable winds handled well)

**Total**: 75 points (Good performance)

### Example 3: Challenging Scenario

**Scenario**: Severe Weather (4 aircraft, 28 minutes)

**Breakdown**:
- Safety: 30 points
  - 2 separation violations: -10 points (only 25 of 40)
  - Altitude management: 10 points
  - Crosswind management: 10 points
  - Landings: 15 points
- Efficiency: 32 points
  - Landing rate: 8.6 aircraft/hour = 25 points
  - Vector quality: Average (12 commands) = 4 points
  - Workload management: 8 points
  - Fuel economy: 2 points (several extra turns)
- Weather bonus: 10 points (severe weather completed)

**Total**: 72 points (Acceptable but room for improvement)

## Performance Ratings

### Overall Rating

| Score | Rating | Level |
|-------|--------|-------|
| 95-100 | S-Rank | Legendary |
| 85-94 | A-Rank | Expert |
| 75-84 | B-Rank | Advanced |
| 65-74 | C-Rank | Intermediate |
| 55-64 | D-Rank | Beginner |
| < 55 | F-Rank | Needs Improvement |

### Comparative Metrics

**Leaderboard Tiers**:
- **Top 1%**: S-Rank scores across multiple scenarios
- **Top 10%**: Consistent A-Rank performance
- **Top 25%**: A/B-Rank mix
- **Top 50%**: B/C-Rank mix
- **Bottom 50%**: C-Rank or lower

## Tips for Maximum Score

### Safety-First Strategy
1. Maintain conservative separation (1.5 nm extra)
2. Avoid altitude limits (maintain 1,000+ ft buffer)
3. Use go-arounds liberally
4. Clear communication

**Expected Score**: 60-70 points

### Efficiency Focus
1. Sequence aircraft effectively
2. Use direct vectors
3. Minimize heading/altitude changes
4. High landing rate

**Expected Score**: 70-80 points

### Balanced Approach
1. Maintain safety standards
2. Improve efficiency progressively
3. Learn optimal patterns
4. Build experience

**Expected Score**: 75-85 points

### Expert Optimization
1. Maximize landing rate
2. Perfect sequencing
3. Zero safety violations
4. Complete mastery

**Expected Score**: 90+ points

## Difficulty Multipliers

Scores adjusted by scenario difficulty:

- **Beginner**: ×1.0 (base)
- **Intermediate**: ×1.2
- **Advanced**: ×1.4
- **Expert**: ×1.6

**Examples**:
```
Beginner scenario, 80 points = 80 points (leaderboard)
Advanced scenario, 80 points = 112 points (leaderboard)
Expert scenario, 80 points = 128 points (leaderboard)
```

## Seasonal Scoring

**Leaderboards reset monthly** with bonus points for:
- Consistent A-Rank performance: +10 points
- New personal records: +5 points
- Scenario master (10+ completions): +5 points per scenario

## See Also

- [Getting Started](GETTING_STARTED.md) — Learn basics
- [Best Practices](BEST_PRACTICES.md) — Improve your score
- [Scenarios](SCENARIOS.md) — Understand scenario-specific scoring
- [FAQ](FAQ.md) — Scoring questions

---

**Note**: Scores are updated in real-time. Target 75+ points for solid performance, 85+ for expert level.
