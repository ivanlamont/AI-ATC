# 🚀 Getting Started with AI-ATC

Welcome to AI-ATC, an air traffic control simulator where you manage aircraft, issue clearances, and guide planes to safe landings. This guide will get you up and running in minutes.

## Prerequisites

- **Computer Requirements**:
  - Processor: Intel Core i5 or equivalent
  - Memory: 8GB RAM minimum (16GB recommended)
  - Storage: 2GB free disk space
  - GPU: Optional (RTX series recommended for faster training)

- **Software**:
  - Python 3.11 or later
  - Docker (optional, for containerized deployment)

## Installation

### Option 1: Direct Python Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/AI-ATC.git
   cd AI-ATC
   ```

2. **Create a virtual environment**:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

4. **Run the simulator**:
   ```bash
   python train_ai_atc.py
   ```

### Option 2: Docker Installation (Recommended)

1. **Build the Docker image**:
   ```bash
   docker build -t ai-atc .
   ```

2. **Run the container**:
   ```bash
   docker run --gpus all -it ai-atc python train_ai_atc.py
   ```

## Your First Session

### Starting the Simulator

```bash
python train_ai_atc.py
```

The simulator will initialize with:
- One airport at your specified position
- Initial weather conditions
- Ready aircraft on approach

### Basic Workflow

1. **Monitor the Radar Display**
   - View all active aircraft
   - Check their current altitude, speed, and heading
   - Identify conflicts or safety concerns

2. **Issue Clearances**
   - Use text commands to give aircraft instructions
   - Commands follow standard ATC phraseology
   - Example: `AAL123 descend to 3000`

3. **Guide Aircraft to Landing**
   - Provide descent clearances
   - Align aircraft with the runway
   - Monitor for safe separation

4. **Observe the Reward**
   - Successfully landed aircraft increase your score
   - Safety violations decrease your score
   - Review feedback at scenario end

## Understanding the Interface

### Radar Display

The main view shows:
- **Aircraft Icons**: Colored dots representing active planes
- **Altitude Callouts**: Current flight level above each aircraft
- **Runway**: Target landing area at bottom
- **Airspace**: Control zone with sector boundaries
- **Weather Info**: Wind speed and direction overlay

### Status Panel

Left side displays:
- **Active Aircraft**: List of planes in your airspace
- **Current Time**: Simulation time (may be accelerated)
- **Wind Conditions**: Speed and direction
- **Score**: Current session performance

### Command Input

Bottom section:
- **Text Input Field**: Enter ATC commands
- **Recent Commands**: History of issued clearances
- **Command Hints**: Autocomplete suggestions

## Basic Commands

The simplest commands to get started:

```
<CALLSIGN> descend to <ALTITUDE>      # Reduce altitude
<CALLSIGN> climb to <ALTITUDE>        # Increase altitude
<CALLSIGN> turn left heading <HEADING>  # Change heading left
<CALLSIGN> turn right heading <HEADING> # Change heading right
<CALLSIGN> reduce speed <SPEED>       # Slow down
<CALLSIGN> increase speed <SPEED>     # Speed up
```

Example:
```
AAL456 descend to 2000 feet
UAL789 turn right heading 090
```

## First Exercise: Simple Approach

**Goal**: Land one aircraft safely

**Steps**:
1. Start a new scenario (Beginner difficulty)
2. Wait for aircraft to appear on radar
3. Issue `descend to 2500`
4. As it approaches, issue `descend to 1500`
5. Align with runway using heading commands
6. Issue final descent `descend to 500`
7. Aircraft should land automatically when aligned

**Success Criteria**:
- No separation violations
- No altitude busts
- Aircraft reaches runway

## Understanding Difficulty Levels

### Beginner
- 1-2 aircraft
- Clear weather
- Simple descent pattern
- Generous separation minimums

### Intermediate
- 3-4 aircraft
- Variable wind
- Complex approach patterns
- Standard separation minimums

### Advanced
- 5-8 aircraft
- Weather changes
- Mixed VFR/IFR traffic
- Strict separation requirements
- Scenario changes mid-flight

### Expert
- 8+ aircraft
- Severe weather
- Runway changes
- Wake turbulence considerations
- Emergency situations

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Space` | Pause/Resume simulation |
| `+/-` | Speed up/slow down simulation |
| `R` | Reset current scenario |
| `H` | Display help |
| `Q` | Quit to main menu |
| `1-5` | Switch scenario difficulty |

See [Keyboard Shortcuts](KEYBOARD_SHORTCUTS.md) for complete reference.

## Tips for Success

1. **Read the Radar Early**: Anticipate conflicts before they develop
2. **Use Descent Profiles**: Avoid rapid altitude changes
3. **Plan Ahead**: Start approach procedures early
4. **Monitor Wind**: Wind affects approach stability
5. **Maintain Separation**: Minimum 1,000 feet vertical or 2 nautical miles horizontal
6. **Use Standard Phraseology**: Follow ATC conventions for clarity

## Common Issues

**Aircraft not responding to commands**
- Check callsign spelling (case-sensitive)
- Verify command syntax
- Ensure aircraft is in controlled airspace

**Scenario too difficult**
- Try lower difficulty level
- Review [Best Practices](BEST_PRACTICES.md)
- Practice approach procedures

**Performance problems**
- Check [Troubleshooting Guide](TROUBLESHOOTING.md)
- Reduce visual quality settings
- Close background applications

## Next Steps

1. **Practice Basic Approaches**: Complete 5 scenarios on Beginner
2. **Learn Command Reference**: Study [ATC Commands](ATC_COMMAND_REFERENCE.md)
3. **Understand Scoring**: Review [Scoring System](SCORING_SYSTEM.md)
4. **Try Intermediate**: Advance to Intermediate difficulty
5. **Read Best Practices**: Apply techniques from [Best Practices](BEST_PRACTICES.md)

## Additional Resources

- [ATC Command Reference](ATC_COMMAND_REFERENCE.md) — Complete command list
- [Scoring System](SCORING_SYSTEM.md) — How scores are calculated
- [Best Practices](BEST_PRACTICES.md) — Expert techniques
- [Troubleshooting](TROUBLESHOOTING.md) — Common problems and solutions
- [FAQ](FAQ.md) — Frequently asked questions

## Getting Help

- **In-Game Help**: Press `H` during simulation
- **Documentation**: Check the docs folder
- **FAQ**: Review [FAQ.md](FAQ.md) for quick answers
- **Issues**: Report problems on GitHub

Good luck, Controller! 🎮✈️
