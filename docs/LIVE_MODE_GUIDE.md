# 🌍 Live Mode Guide - Real Aircraft Integration

AI-ATC Live Mode connects you with real-time aircraft data from ADSBexchange, allowing you to control actual aircraft approaching major airports.

## What is Live Mode?

Live Mode integrates real ADS-B tracking data to create realistic scenarios with actual aircraft. Instead of simulated traffic, you manage real aircraft currently airborne near your selected airport.

### Features

- **Real Aircraft Data**: Live positions, altitudes, speeds from ADSBexchange
- **Authentic Traffic**: See actual airline traffic patterns
- **Learning Opportunity**: Practice with real-world scenarios
- **Global Coverage**: Works with any major airport worldwide
- **Hybrid Scenarios**: Mix real and simulated aircraft

## Getting Started with Live Mode

### Enable Live Mode

1. **Start AI-ATC**
2. **Select "Live Mode" from main menu**
3. **Choose Airport**:
   - SFO - San Francisco
   - LAX - Los Angeles
   - JFK - New York
   - ORD - Chicago
   - ATL - Atlanta

4. **Customize Settings**:
   ```
   Search Radius: 30-50 nm
   Approaching Only: Yes (recommended)
   Maximum Aircraft: 10-20
   Update Interval: 10 seconds
   ```

5. **Start Scenario**

## Live Mode Features

### Real Aircraft Display

Live aircraft are marked distinctly:
- **Real Aircraft**: Blue callsign with real icon
- **Simulated Aircraft**: Green callsign with generic icon
- **Mixed Scenarios**: Both types displayed

### Live Data Information

Each aircraft shows:
- **Callsign**: Real flight number (e.g., "AAL456")
- **Aircraft Type**: Actual aircraft model
- **Position**: Real latitude/longitude
- **Altitude**: Current flight level
- **Speed**: Ground speed in knots
- **Track**: Current heading
- **Origin/Destination**: Actual flight plan
- **Operator**: Airline name

### Real-Time Updates

- Updates every 10 seconds by default
- Configurable update frequency
- Intelligent caching to reduce API calls
- Graceful degradation on network issues

## Scenario Modes

### Approaching Only

Shows only aircraft descending toward the airport.

**Benefits**:
- Realistic approach scenarios
- Smaller number of aircraft to manage
- Focus on landing operations

**Example**:
```
SFO Approaching Only
├── UAL456 (B737): 35nm, 8000ft, descending
├── AAL789 (A320): 20nm, 5000ft, descending
└── DAL123 (B767): 45nm, 12000ft, descending
```

### Regional

Shows all aircraft in a geographic region.

**Benefits**:
- Mix of arrivals and departures
- Complex traffic patterns
- Realistic approach sequences

### High Density

Maximum aircraft in area.

**Benefits**:
- Expert-level challenge
- Realistic for major hubs
- Ultimate workload test

## Understanding Real Aircraft

### Aircraft Types Commonly Seen

| Type | Airline | Size | Speed |
|------|---------|------|-------|
| B737 | Southwest, United | Medium | 450 kt |
| A320 | American, Delta | Medium | 460 kt |
| B777 | Delta, United | Large | 490 kt |
| A350 | International | Large | 500 kt |
| E175 | Regional | Small | 380 kt |
| CRJ9 | Regional | Small | 350 kt |

### Reading Real Flight Data

**Callsign Interpretation**:
- **AAL456**: American Airlines flight 456
- **UAL789**: United Airlines flight 789
- **SWR123**: Southwest flight 123

**Destination Codes**:
- **SFO**: San Francisco International
- **LAX**: Los Angeles International
- **JFK**: New York JFK
- **ORD**: Chicago O'Hare
- **ATL**: Hartsfield-Jackson Atlanta

### Performance Expectations

Real aircraft have realistic performance:
- Typical cruise speed: 430-500 knots
- Typical descent rate: 1500-2000 fpm
- Typical approach speed: 150-170 knots
- Typical landing distance: 5000-6000 feet

## Controlling Real Aircraft

### Standard Commands Work

All normal ATC commands apply to real aircraft:

```
AAL456 descend to 3000 feet
UAL789 turn right heading 270
SWR123 reduce speed to 140 knots
```

### Real-World Behavior

Real aircraft follow:
- Performance limitations
- Realistic rates of change
- Actual flight plans (where available)
- Realistic communication delays

### Important Notes

- **Not Disruptive**: Commands are simulated; real aircraft unaffected
- **Educational**: Understand real procedures and limitations
- **Safe Practice**: No impact on actual flying

## Difficulty Levels in Live Mode

### Beginner (Live)
- 1-2 real aircraft
- Approaching only
- Clear weather simulation
- 15 minute scenario

### Intermediate (Live)
- 3-5 real aircraft
- Mixed arrivals/departures
- Variable weather
- 25 minute scenario

### Advanced (Live)
- 5-8 real aircraft
- Complex traffic patterns
- Challenging weather
- 35 minute scenario

### Expert (Live)
- 8-12 real aircraft
- High density traffic
- Severe weather
- 45+ minute scenario

## Tips for Live Mode Success

### 1. Respect Real Performance

Real aircraft have actual limitations:
- Can't climb/descend unrealistically fast
- Can't turn sharply at high speed
- Need realistic distances for approaches

### 2. Use Real-World Procedures

Approach procedures based on actual STARs:
```
Example: SFO MARBL Approach
- Enter at MARBL fix: 6,000 ft
- Descend to TRONC: 3,000 ft
- Continue to MADBE: 1,500 ft
- Final approach to runway
```

### 3. Manage Multiple Airlines

Real traffic has airline mix:
```
Typical SFO Hour:
- United: 3 aircraft (hub carrier)
- American: 2 aircraft
- Southwest: 2 aircraft
- International: 1-2 aircraft
```

### 4. Watch for Real Constraints

Weather affects real aircraft:
- Headwind/tailwind impacts landing distance
- Visibility affects approach difficulty
- Wind shear can cause go-arounds

## Troubleshooting Live Mode

### "No Aircraft Available"

**Causes**:
- Time of day with light traffic
- Airport with few arrivals
- Search radius too small

**Solutions**:
- Try different airport
- Increase search radius to 50nm
- Increase search time window
- Add simulated aircraft to scenario

### "Aircraft Disappeared"

**Causes**:
- Landed or departed area
- Signal lost
- ADS-B receiver outage

**Solutions**:
- Normal occurrence in real data
- Add simulated aircraft to fill gap
- Accept realistic variability
- Refresh aircraft list

### "High Latency/Lag"

**Causes**:
- API rate limit exceeded
- Network connectivity
- Too many aircraft tracked
- High update frequency

**Solutions**:
- Increase update interval to 20-30 seconds
- Reduce maximum aircraft count
- Check internet connection
- Reduce search radius

### "Inconsistent Aircraft Data"

**Causes**:
- Aircraft doesn't respond to all transmitters
- Data gaps in real system
- Multiple transmitters disagree

**Solutions**:
- Normal for real ADS-B data
- Accept occasional data inconsistencies
- Add simulated aircraft for consistency
- Use "clean up" feature to remove outliers

## Privacy & Ethics

### Data Source
- **ADSBexchange**: Public aviation data
- **Publicly Available**: Same data used by FlightRadar24, FlightAware
- **Voluntary**: Pilots can disable ADS-B broadcast

### Ethical Use
- ✅ **Allowed**: Personal learning and practice
- ✅ **Allowed**: Educational research
- ✅ **Allowed**: Hobbyist simulation
- ❌ **Not Allowed**: Commercial without permission
- ❌ **Not Allowed**: Tracking specific aircraft
- ❌ **Not Allowed**: Privacy violation

## Advanced Features

### Hybrid Scenarios

Mix real and simulated aircraft:

```python
from live_data import create_live_scenario_for_airport
from live_data import Aircraft

scenario = create_live_scenario_for_airport('SFO')

# Get real aircraft
real_aircraft = scenario.get_live_aircraft()

# Add simulated aircraft
simulated = Aircraft(
    icao='SIMUL1',
    callsign='SIM001',
    latitude=37.5,
    longitude=-122.4,
    altitude_ft=5000,
    # ... other params
)
scenario.add_simulated_aircraft(simulated)
```

### Custom Scenarios

Create scenarios with mixed real/simulated:

```python
from live_data import LiveModeManager, LiveScenarioConfig

manager = LiveModeManager()

config = LiveScenarioConfig(
    airport_code='LAX',
    fetch_approaching_only=True,
    max_aircraft_count=10,
    scale_factor=1.0,
)

scenario = manager.create_scenario(config)
```

### API Integration

Integrate live data into your app:

```python
from live_data import ADSBClient

client = ADSBClient()

# Get aircraft near airport
aircraft = client.get_aircraft_by_airport('SFO', radius_nm=30)

# Process aircraft
for ac in aircraft:
    print(f"{ac.callsign}: {ac.altitude_ft} ft")

# Cache status
status = client.get_cache_status()
print(f"Remaining API calls: {status['rate_limit_remaining']}")
```

## Limitations & Future

### Current Limitations
- Stops working when API unavailable
- Limited to 5 major US airports
- Requires internet connection
- Real-time data lag (typically 1-5 seconds)
- No audio/text data integration yet

### Future Enhancements
- [ ] LiveATC audio integration
- [ ] World-wide airport support
- [ ] Departure traffic inclusion
- [ ] Weather integration
- [ ] Real flight plan integration
- [ ] Aircraft type filtering
- [ ] Airline-specific scenarios
- [ ] Time-travel replay of historical data

## Performance Impact

### System Requirements
- Internet connection (5+ Mbps)
- API call budget (~100/hour)
- Cache size (~10-50 MB for typical airport)

### Optimization Tips
- Increase update interval to 20+ seconds
- Reduce search radius to 30nm
- Limit aircraft count to 10-15
- Use "approaching only" mode

## See Also

- [Airport Scenarios](AIRPORT_SCENARIOS.md) — Standard scenarios
- [Getting Started](GETTING_STARTED.md) — Basic gameplay
- [Best Practices](BEST_PRACTICES.md) — Expert techniques
- [Troubleshooting](TROUBLESHOOTING.md) — Common issues

## Legal Disclaimer

- AI-ATC Live Mode is for entertainment and educational purposes
- Real aircraft are not affected by simulated commands
- ADS-B data is publicly available
- Use respects aviation data sharing policies
- Not affiliated with airlines or air traffic control authorities

---

**Ready to Try Live Mode?** Start with SFO Approaching scenario! 🌍✈️
