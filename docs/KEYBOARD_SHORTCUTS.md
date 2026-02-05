# ⌨️ Keyboard Shortcuts

Complete reference of keyboard shortcuts in AI-ATC.

## Simulation Control

### Playback

| Shortcut | Action |
|----------|--------|
| **Space** | Pause / Resume simulation |
| **Plus (+)** | Speed up simulation (1x → 2x → 4x) |
| **Minus (-)** | Slow down simulation (1x → 0.5x → 0.25x) |
| **1** | 1x speed (normal) |
| **2** | 2x speed (fast) |
| **4** | 4x speed (very fast) |

### Scenario Control

| Shortcut | Action |
|----------|--------|
| **R** | Reset current scenario |
| **N** | Load next scenario |
| **P** | Load previous scenario |
| **Q** | Quit to main menu |
| **X** | Exit application |

## Display and Interface

### View Control

| Shortcut | Action |
|----------|--------|
| **V** | Cycle view modes (Radar → 3D → Map) |
| **Z** | Zoom in |
| **Shift+Z** | Zoom out |
| **0** | Reset zoom to default |
| **C** | Center view on airport |
| **T** | Toggle aircraft trails |
| **L** | Toggle labels (callsigns/altitudes) |

### Radar Display

| Shortcut | Action |
|----------|--------|
| **Tab** | Cycle highlighted aircraft |
| **Shift+Tab** | Cycle in reverse |
| **F** | Focus on selected aircraft |
| **E** | Show expanded aircraft info |
| **M** | Show aircraft on map |

### Information Panels

| Shortcut | Action |
|----------|--------|
| **I** | Toggle info panel |
| **S** | Toggle status bar |
| **W** | Show weather details |
| **D** | Show diagnostic info |
| **K** | Show this keyboard help |

## Command Entry

### Text Input

| Shortcut | Action |
|----------|--------|
| **Tab** | Autocomplete callsign |
| **Shift+Tab** | Previous autocomplete option |
| **Up Arrow** | Previous command history |
| **Down Arrow** | Next command history |
| **Backspace** | Delete character |
| **Ctrl+A** | Select all |
| **Ctrl+X** | Cut |
| **Ctrl+C** | Copy |
| **Ctrl+V** | Paste |
| **Enter** | Submit command |
| **Escape** | Clear input |

### Common Command Shortcuts

| Shortcut | Expands To |
|----------|------------|
| **D** + **Tab** | Descend to |
| **C** + **Tab** | Climb to |
| **T** + **Tab** | Turn |
| **R** + **Tab** | Reduce speed |
| **I** + **Tab** | Increase speed |

Examples:
```
D<TAB> = Descend to (then type altitude)
T<TAB> = Turn (then type direction/heading)
```

## Difficulty and Settings

### Difficulty Selection (During Menu)

| Shortcut | Action |
|----------|--------|
| **1** | Beginner difficulty |
| **2** | Intermediate difficulty |
| **3** | Advanced difficulty |
| **4** | Expert difficulty |
| **R** | Random difficulty |

### Settings

| Shortcut | Action |
|----------|--------|
| **O** | Open options menu |
| **G** | Graphics settings |
| **A** | Audio settings |
| **B** | Gameplay settings |
| **K** | Keyboard/Controls settings |

## Accessibility

### Accessibility Options

| Shortcut | Action |
|----------|--------|
| **Ctrl+Plus** | Increase font size |
| **Ctrl+Minus** | Decrease font size |
| **Ctrl+U** | Toggle high contrast mode |
| **Ctrl+H** | Toggle audio descriptions |

## Advanced Features

### Recording and Replay

| Shortcut | Action |
|----------|--------|
| **Shift+R** | Start/Stop recording session |
| **Shift+P** | Play recorded session |
| **Shift+D** | Delete last recording |
| **Ctrl+Shift+R** | Export recording as video |

### Analysis Tools

| Shortcut | Action |
|----------|--------|
| **Ctrl+1** | Show performance metrics |
| **Ctrl+2** | Show separation graph |
| **Ctrl+3** | Show workload analysis |
| **Ctrl+4** | Show wind analysis |
| **Ctrl+5** | Show detailed statistics |

## VFR/IFR Specific

### Flight Following (VFR)

| Shortcut | Action |
|----------|--------|
| **Alt+F** | Toggle flight following list |
| **Alt+T** | Show traffic advisories |
| **Alt+V** | Show VFR procedures |

### IFR Specific

| Shortcut | Action |
|----------|--------|
| **Alt+I** | Toggle IFR mode |
| **Alt+A** | Show approach procedures |
| **Alt+H** | Show holding patterns |

## Emergency Procedures

### Emergency Responses

| Shortcut | Action |
|----------|--------|
| **Ctrl+E** | Declare emergency |
| **Ctrl+G** | Issue go-around (selected aircraft) |
| **Ctrl+M** | Mayday call |
| **Ctrl+U** | Urgent communication |

## Help and Documentation

### Help System

| Shortcut | Action |
|----------|--------|
| **H** | Show main help |
| **F1** | Context-sensitive help |
| **?** | Quick reference guide |
| **Ctrl+?** | Full documentation index |

### Tutorials and Learning

| Shortcut | Action |
|----------|--------|
| **Shift+T** | Start tutorial |
| **Shift+L** | Show lessons menu |
| **Shift+G** | Launch guided scenario |

## Joystick/Controller (If Supported)

### Button Mapping

| Button | Action |
|--------|--------|
| **A / Cross** | Submit command |
| **B / Circle** | Cancel command |
| **Y / Triangle** | Open menu |
| **X / Square** | Help/Info |
| **LB** | Previous aircraft |
| **RB** | Next aircraft |
| **LT** | Slow down |
| **RT** | Speed up |
| **DPad Up** | Zoom in |
| **DPad Down** | Zoom out |
| **DPad Left** | Previous view |
| **DPad Right** | Next view |

## Customizing Shortcuts

### Editing Keybindings

1. Open settings: Press **O**
2. Select "Keyboard/Controls"
3. Choose shortcut to modify
4. Press new key combination
5. Click "Bind" to confirm

### Configuration File

Edit directly in: `~/.claude/keybindings.json`

**Format**:
```json
{
  "pause": "Space",
  "speed_up": "Plus",
  "speed_down": "Minus",
  "reset": "R",
  "help": "H"
}
```

### Resetting to Defaults

```bash
python train_ai_atc.py --reset-keybindings
```

## Keyboard Layouts

### QWERTY (Default)

All shortcuts listed above.

### DVORAK

Shortcuts automatically adapt to your system keyboard layout.

### International Layouts

Most shortcuts work with local keyboard layouts. Some may require adjustment.

## Tips

### Efficient Command Entry

**Combination**: `Ctrl+A` + `Ctrl+X` + command + `Enter`
- Selects all previous text
- Cuts it
- Types new command
- Submits

### Speed Tips

1. **Autocomplete**: Use **Tab** to complete callsigns quickly
2. **History**: Use **Up Arrow** to repeat previous commands
3. **Quick zoom**: **Z** to zoom, **0** to reset
4. **Speed control**: Use **Space** to pause for planning

### Emergency Response

Emergency procedure shortcut chain:
1. **Space** — Pause (buy time)
2. **Ctrl+E** — Declare emergency
3. **Ctrl+G** — Issue go-arounds to other aircraft
4. **Space** — Resume simulation

## Common Shortcut Combinations

### Quick Analysis

1. **Space** — Pause
2. **Ctrl+1** — Show metrics
3. **Ctrl+2** — Show separation graph
4. **Space** — Resume

### Record and Export

1. **Shift+R** — Start recording
2. Complete scenario
3. **Shift+R** — Stop recording
4. **Ctrl+Shift+R** — Export as video

### View Switch

1. **V** — Switch view (Radar)
2. **V** — Switch view (3D)
3. **V** — Switch view (Map)
4. **V** — Back to Radar

## Accessibility Shortcuts

### High Contrast Mode

1. **Ctrl+U** — Toggle contrast
2. **Ctrl+Plus** — Increase text
3. **Ctrl+H** — Enable audio descriptions

### Reduced Motion

In settings, enable "Reduce Motion" for smoother animations without flashing.

## See Also

- [Getting Started](GETTING_STARTED.md) — Tutorial for new users
- [ATC Commands](ATC_COMMAND_REFERENCE.md) — All valid commands
- [FAQ](FAQ.md) — Common questions

---

**Tip**: Press **K** during simulation to display keyboard shortcuts overlay.
