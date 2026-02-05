# 📋 ATC Command Reference

This reference documents all valid ATC commands in AI-ATC. Commands follow standard aviation phraseology for clarity and realism.

## Command Syntax

All commands follow this basic pattern:
```
<CALLSIGN> <INSTRUCTION> <PARAMETER>
```

**Components**:
- **CALLSIGN**: Aircraft identifier (e.g., AAL456, UAL789)
- **INSTRUCTION**: Action to take (e.g., descend, climb)
- **PARAMETER**: Specific value for the action

## Altitude Commands

Commands for changing aircraft altitude.

### Descend

Instruct aircraft to descend to a specific altitude.

**Syntax**: `<CALLSIGN> descend to <ALTITUDE_FT>`

**Examples**:
```
AAL456 descend to 3000
UAL789 descend to 2000 feet
SKW234 descend to flight level 180
```

**Notes**:
- Altitude in feet or flight level (FL180 = 18,000 ft)
- Aircraft will descend at standard rate (200-300 fpm)
- Cannot descend below 500 feet above ground

### Climb

Instruct aircraft to climb to a specific altitude.

**Syntax**: `<CALLSIGN> climb to <ALTITUDE_FT>`

**Examples**:
```
AAL456 climb to 5000
UAL789 climb to FL250
```

**Notes**:
- Aircraft will climb at standard rate (500-800 fpm)
- Cannot climb above aircraft maximum altitude
- Climb rate limited by aircraft type

### Maintain Altitude

Instruct aircraft to maintain current altitude.

**Syntax**: `<CALLSIGN> maintain <ALTITUDE_FT>`

**Examples**:
```
AAL456 maintain 3000
UAL789 maintain flight level 200
```

**Notes**:
- Used to stabilize aircraft at current level
- Prevents unwanted altitude changes
- Essential for approach stability

## Heading Commands

Commands for changing aircraft direction.

### Turn Left

Instruct aircraft to turn left to a specific heading.

**Syntax**: `<CALLSIGN> turn left heading <HEADING_DEGREES>`

**Examples**:
```
AAL456 turn left heading 270
UAL789 turn left heading 090
```

**Notes**:
- Heading in degrees magnetic (0-360)
- Aircraft turns at standard rate (~2-3° per second)
- "Left" specifies shortest turn direction

### Turn Right

Instruct aircraft to turn right to a specific heading.

**Syntax**: `<CALLSIGN> turn right heading <HEADING_DEGREES>`

**Examples**:
```
AAL456 turn right heading 090
UAL789 turn right heading 180
```

**Notes**:
- Standard rate turn (~2-3° per second)
- Provides stable approach geometry

### Turn Heading

Turn to heading (shortest direction).

**Syntax**: `<CALLSIGN> turn heading <HEADING_DEGREES>`

**Examples**:
```
AAL456 turn heading 270
UAL789 turn heading 045
```

**Notes**:
- System selects left or right automatically
- Typically shorter turn distance

### Direct

Direct aircraft to fly directly to a waypoint/fix.

**Syntax**: `<CALLSIGN> direct <WAYPOINT>`

**Examples**:
```
AAL456 direct airport
UAL789 direct approach fix
```

**Notes**:
- Aircraft turns to intercept direct course
- Useful for approach alignment

## Speed Commands

Commands for changing aircraft speed.

### Reduce Speed

Reduce aircraft speed.

**Syntax**: `<CALLSIGN> reduce speed to <SPEED_KNOTS>`

**Examples**:
```
AAL456 reduce speed to 150 knots
UAL789 reduce speed 140
```

**Notes**:
- Speed in knots
- Can't reduce below aircraft minimum
- Typical minimum 100-120 knots

### Increase Speed

Increase aircraft speed.

**Syntax**: `<CALLSIGN> increase speed to <SPEED_KNOTS>`

**Examples**:
```
AAL456 increase speed to 180 knots
UAL789 increase speed 200
```

**Notes**:
- Cannot exceed aircraft maximum
- Typical maximum 250-450 knots
- Speed limited by airspace and phase

### Maintain Speed

Maintain current speed.

**Syntax**: `<CALLSIGN> maintain <SPEED_KNOTS>`

**Examples**:
```
AAL456 maintain 160 knots
UAL789 maintain 180
```

## Approach Commands

Commands for approach procedures.

### Vector for Approach

Set aircraft up for approach to runway.

**Syntax**: `<CALLSIGN> vector for approach runway <RUNWAY>`

**Examples**:
```
AAL456 vector for approach runway 27
UAL789 vector for approach runway 09L
```

**Notes**:
- Aligns aircraft for landing
- May require multiple commands
- Aircraft should be at approach altitude

### Cleared to Land

Clear aircraft for landing.

**Syntax**: `<CALLSIGN> cleared to land runway <RUNWAY>`

**Examples**:
```
AAL456 cleared to land runway 27
UAL789 cleared to land runway 09
```

**Notes**:
- Only issue when aligned and ready
- Aircraft must be below 1,500 feet
- Wind must be within limits

## Combined Commands

Combine multiple instructions in one command.

**Syntax**: `<CALLSIGN> <INSTRUCTION1> and <INSTRUCTION2>`

**Examples**:
```
AAL456 descend to 2000 and turn right heading 270
UAL789 reduce speed to 140 and maintain heading 090
```

**Notes**:
- Reduces radio calls
- Both actions execute simultaneously
- Improves realism

## Conditional Commands

Commands with conditions (based on scenario).

### Speed Restriction at Waypoint

Speed restriction at specific location.

**Syntax**: `<CALLSIGN> speed restricted to <SPEED> at <WAYPOINT>`

**Examples**:
```
AAL456 speed restricted to 180 at outer marker
UAL789 speed restricted to 150 at approach fix
```

## Clearance Commands

Commands for issuing clearances.

### Flight Following

Provide flight following service.

**Syntax**: `<CALLSIGN> flight following approved`

**Examples**:
```
N12345 flight following approved
SKW234 flight following approved
```

**Notes**:
- VFR aircraft use flight following
- Different from radar vectoring

### Approach Clearance

Issue approach clearance.

**Syntax**: `<CALLSIGN> approach clearance approved`

**Examples**:
```
AAL456 approach clearance approved
UAL789 approach clearance approved
```

## Emergency Commands

Commands for emergency situations.

### Go Around

Instruct aircraft to abort landing.

**Syntax**: `<CALLSIGN> go around`

**Examples**:
```
AAL456 go around
UAL789 go around maintain heading 270
```

**Notes**:
- Aircraft climbs to safe altitude
- Turns away from runway
- Provides separation buffer

### Declare Emergency

Handle aircraft emergency.

**Syntax**: `<CALLSIGN> emergency <TYPE>`

**Examples**:
```
AAL456 emergency hydraulic failure
UAL789 emergency engine fire
```

**Notes**:
- Triggers special handling
- Priority clearances
- Enhanced monitoring

## Configuration Commands

Commands for changing scenario settings.

### Change Runway

Switch active runway.

**Syntax**: `change runway to <RUNWAY>`

**Examples**:
```
change runway to 27L
change runway to 09
```

**Notes**:
- Only during appropriate times
- Affects all aircraft
- Limited number of changes per scenario

### Update Weather

Change weather conditions (scenario control).

**Syntax**: `set weather wind <SPEED> from <DIRECTION>`

**Examples**:
```
set weather wind 15 from 270
set weather wind 20 from 090
```

## Command Tips

### Effective Phrasing

- **Clear**: `AAL456 descend to 2000`
- **Unclear**: `go lower`

### Common Patterns

**Approach Setup**:
1. `descend to 3000`
2. `turn right heading 270`
3. `descend to 2000`
4. `reduce speed to 140`
5. `descend to 500`
6. `cleared to land`

**Go Around Recovery**:
1. `go around`
2. `climb to 2000`
3. `turn left heading 090`
4. `reduce speed to 150`

### Abbreviations

Common abbreviations accepted:
- **FT** = Feet
- **FL** = Flight level
- **KTS** = Knots
- **HDG** = Heading
- **ALT** = Altitude
- **SPD** = Speed

## Error Handling

### Invalid Commands

If a command fails:
- **Message**: "Unable to comply"
- **Reason**: Displayed on screen
- **Resolution**: Check [FAQ](FAQ.md) or [Troubleshooting](TROUBLESHOOTING.md)

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| "Unknown callsign" | Misspelled aircraft ID | Check active aircraft list |
| "Unable to comply" | Command invalid for state | Read error message |
| "Insufficient altitude" | Descent too low | Request higher altitude |
| "Speed limit exceeded" | Too fast for conditions | Reduce speed |

## Advanced Techniques

### Sequencing Aircraft

Queue aircraft for landing:
```
AAL456 descend to 2000
UAL789 descend to 3000
SKW234 descend to 4000
```

### Holding Patterns

Maintain altitude and heading:
```
AAL456 maintain 3000
AAL456 hold heading 090
```

### Separation Management

Use altitude/speed for separation:
```
AAL456 descend to 2000, reduce speed 140
UAL789 maintain 3000, increase speed 160
```

## Keyboard Input Tips

- **Tab**: Autocomplete callsigns
- **Up Arrow**: Previous command history
- **Backspace**: Delete last character
- **Enter**: Submit command

## See Also

- [Getting Started](GETTING_STARTED.md) — New user guide
- [Best Practices](BEST_PRACTICES.md) — Expert techniques
- [Scoring System](SCORING_SYSTEM.md) — Command impact on score
- [FAQ](FAQ.md) — Common questions

---

**Note**: Command syntax is case-insensitive. Altitude defaults to feet; use "FL" prefix for flight levels.
