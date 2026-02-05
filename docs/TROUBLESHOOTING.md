# 🔧 Troubleshooting Guide

Solutions to common problems in AI-ATC.

## Installation Issues

### Problem: "Python not found" or "ModuleNotFoundError"

**Symptoms**:
- `python: command not found`
- `ModuleNotFoundError: No module named 'gymnasium'`
- `ImportError: cannot import name 'stable_baselines3'`

**Solutions**:

1. **Install Python**:
   - Download from https://www.python.org/
   - Verify: `python --version` (should be 3.11+)

2. **Create virtual environment**:
   ```bash
   python -m venv venv
   source venv/bin/activate  # macOS/Linux
   venv\Scripts\activate     # Windows
   ```

3. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

4. **Verify installation**:
   ```bash
   python -c "import gymnasium; print('OK')"
   ```

### Problem: "Permission denied" on macOS/Linux

**Symptoms**:
- `Permission denied` when running script
- Unable to create files

**Solutions**:

1. **Make script executable**:
   ```bash
   chmod +x train_ai_atc.py
   ```

2. **Fix permissions**:
   ```bash
   sudo chown -R $USER ~/.local/share/ai-atc/
   ```

3. **Run with proper permissions**:
   ```bash
   python train_ai_atc.py
   ```

## Performance Issues

### Problem: Slow Performance / Low FPS

**Symptoms**:
- Jerky/laggy display
- Commands lag behind
- Simulation runs slowly

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Background apps running | Close unnecessary applications |
| High graphics settings | Reduce resolution or quality |
| CPU overloaded | Close other programs, reduce aircraft count |
| GPU drivers outdated | Update graphics drivers |
| Insufficient RAM | Close background apps, use fewer aircraft |

**Quick fixes**:
```bash
# Reduce graphics quality
python train_ai_atc.py --quality low

# Reduce aircraft count
python train_ai_atc.py --max-aircraft 3

# Enable optimization
python train_ai_atc.py --optimize
```

### Problem: High CPU Usage

**Symptoms**:
- CPU at 100%
- Fan noise
- Computer slow/unresponsive

**Solutions**:

1. **Reduce traffic**:
   ```bash
   python train_ai_atc.py --max-aircraft 2
   ```

2. **Lower graphics**:
   ```bash
   python train_ai_atc.py --graphics minimal
   ```

3. **Enable power save mode**:
   ```bash
   python train_ai_atc.py --power-save
   ```

4. **Check processes**:
   ```bash
   # Windows: Task Manager
   # Mac: Activity Monitor
   # Linux: top or htop
   ```

### Problem: Out of Memory

**Symptoms**:
- Game crashes
- "MemoryError" in console
- System freezes

**Solutions**:

1. **Reduce aircraft count**:
   ```bash
   python train_ai_atc.py --max-aircraft 2
   ```

2. **Reduce scenario complexity**:
   ```bash
   python train_ai_atc.py --difficulty beginner
   ```

3. **Disable features**:
   ```bash
   python train_ai_atc.py --no-trails --no-history
   ```

4. **Check available memory**:
   ```bash
   # Linux: free -h
   # Windows: tasklist /v
   # Mac: vm_stat
   ```

## Command Issues

### Problem: "Unknown callsign" Error

**Symptoms**:
- Command rejected
- "Unknown callsign" message
- Aircraft not responding

**Causes & Solutions**:

1. **Check callsign spelling**:
   - Callsigns are case-sensitive
   - Verify against aircraft list
   - Look for typos

2. **Verify aircraft in range**:
   - Check radar display
   - Ensure aircraft not already landed
   - Confirm in controlled airspace

3. **Example**:
   ```
   Wrong: aal456 descend to 3000    (lowercase)
   Right: AAL456 descend to 3000    (uppercase)

   Wrong: AL456 descend to 3000     (typo)
   Right: AAL456 descend to 3000    (correct)
   ```

### Problem: "Unable to comply" Error

**Symptoms**:
- Command rejected
- "Unable to comply" message
- No explanation given

**Causes & Solutions**:

| Error | Cause | Solution |
|-------|-------|----------|
| "Invalid altitude" | Altitude below 500 ft | Request higher altitude |
| "Too high" | Exceeds aircraft max | Request lower altitude |
| "Speed limit exceeded" | Speed too high for airspace | Reduce speed request |
| "Below minimum speed" | Speed too slow | Increase speed request |
| "Crosswind exceeded" | Too much crosswind | Try different runway |
| "Wind limit exceeded" | Runway not safe | Wait for wind change |

**Examples**:
```
Error: "Crosswind exceeded"
Solution: AAL456 descend to 3000, reduce speed to 120
(allows more time to set up approach)
```

### Problem: Command Syntax Not Working

**Symptoms**:
- Command not accepted
- "Invalid command" message
- Help doesn't clarify

**Solutions**:

1. **Check syntax**:
   ```
   Wrong: "AAL456 down 3000"
   Right: "AAL456 descend to 3000"
   ```

2. **Review [ATC Command Reference](ATC_COMMAND_REFERENCE.md)**

3. **Try variations**:
   ```
   AAL456 descend to 3000 feet
   AAL456 descend 3000
   AAL456 descend FL300
   ```

4. **Use autocomplete** (Tab key)

## Aircraft Landing Issues

### Problem: Aircraft Won't Land

**Symptoms**:
- Aircraft passes runway
- Doesn't land despite clearance
- Circles around airport

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Not aligned with runway | Issue heading command to align |
| Wrong altitude | Descend further (below 500 ft) |
| Too fast | Reduce speed to 100-120 knots |
| Crosswind too high | Issue go-around, try different runway |
| Already landed | It successfully landed! Check status |

**Checklist**:
- [ ] Heading within 5° of runway
- [ ] Altitude below 500 ft
- [ ] Speed 100-120 knots
- [ ] Crosswind within limits
- [ ] Wind from right direction

**Example solution**:
```
Problem: Aircraft passes runway
Solution:
1. AAL456 go around, climb to 2000
2. AAL456 turn left heading 270 (align with runway)
3. AAL456 descend to 1500
4. AAL456 reduce speed to 110 knots
5. AAL456 descend to 500
6. AAL456 cleared to land
```

### Problem: Aircraft Crashes on Landing

**Symptoms**:
- Aircraft hits ground
- "Collision with terrain" message
- Scenario ends abruptly

**Causes & Solutions**:

1. **Altitude too low during approach**:
   - Minimum safe: 500 ft at runway
   - Keep above terrain during turns

2. **Turning too sharply**:
   - Issue gradual heading changes
   - Maintain altitude during turns

3. **Speed too high**:
   - Reduce to 100-120 knots for landing
   - Too fast = longer landing distance

4. **Wind conditions**:
   - Check for excessive crosswind
   - Confirm wind within limits

**Prevention**:
- Use "go around" if uncertain
- Maintain safe altitude during approach
- Build in safety margins

### Problem: Aircraft Keeps Going Around

**Symptoms**:
- Multiple go-arounds
- Never lands
- Scenario time running out

**Causes & Solutions**:

1. **Check aircraft limits**:
   ```bash
   # Review aircraft specs
   # Max crosswind: typically 15 knots
   # Min safe altitude: 500 feet
   ```

2. **Simplify approach**:
   ```
   1. AAL456 climb to 3000 (reset altitude)
   2. AAL456 turn heading 270 (simple alignment)
   3. AAL456 descend to 1500
   4. AAL456 reduce speed to 110
   5. AAL456 descend to 500
   6. AAL456 cleared to land
   ```

3. **Check wind**:
   - If tailwind, try different runway
   - If crosswind excessive, wait for wind shift

4. **Manual issue verification**:
   - Are commands valid?
   - Is sequence logical?
   - Are minimums respected?

## Separation and Safety

### Problem: Separation Violations

**Symptoms**:
- Warning message: "Separation violation!"
- Score penalty (-5 points)
- Aircraft too close

**Causes & Solutions**:

1. **Horizontal separation** (< 2 nm):
   - Use altitude separation: 1,000 ft spacing
   - Stagger descents by altitude
   - Plan ahead

2. **Vertical separation** (< 1,000 ft):
   - Descend/climb by 1,500+ feet
   - Allow time for aircraft to execute
   - Don't issue simultaneous level-offs

3. **Prevention**:
   ```
   Safe:
   - AAL456: Descending to 3000 ft
   - UAL789: Maintain 4500 ft

   Unsafe:
   - AAL456: Descend to 3500 ft
   - UAL789: Descend to 3400 ft
   (Vertical separation only 100 ft!)
   ```

**Solution**:
- Issue go-arounds immediately
- Re-sequence with larger spacing
- Use both vertical and horizontal separation

### Problem: Wind Shear Alert

**Symptoms**:
- "Wind shear detected" warning
- Wind suddenly changes
- Aircraft unstable

**Solutions**:

1. **Issue go-around**:
   ```
   AAL456 go around, climb to 2000
   ```

2. **Wait for conditions**:
   - Don't approach during wind shear
   - Wait 1-2 minutes
   - Check wind conditions

3. **Try different runway**:
   ```
   change runway to 09L
   ```

4. **Adjust sequence**:
   - Space aircraft further apart
   - Increase separation margins

## Scenario Issues

### Problem: Scenario Too Difficult

**Symptoms**:
- Too many aircraft to manage
- Frequent violations
- Feeling overwhelmed

**Solutions**:

1. **Reduce difficulty**:
   ```bash
   python train_ai_atc.py --difficulty beginner
   ```

2. **Practice scenarios**:
   - Start with single-aircraft landing
   - Progress to two-aircraft sequential
   - Build skills progressively

3. **Review [Best Practices](BEST_PRACTICES.md)**

4. **Take breaks**:
   - Fatigue affects performance
   - Practice 30 minutes at a time
   - Return refreshed

### Problem: Scenario Too Easy

**Symptoms**:
- Bored with difficulty level
- Perfect scores consistently
- Want more challenge

**Solutions**:

1. **Increase difficulty**:
   ```bash
   python train_ai_atc.py --difficulty advanced
   ```

2. **Add custom challenges**:
   ```bash
   python train_ai_atc.py --custom --aircraft 6 --wind 20
   ```

3. **Set personal goals**:
   - Achieve 95+ score
   - Zero violations
   - Minimize time

4. **Try Expert scenarios**:
   - High-density traffic
   - Emergency procedures
   - 24/7 operations

### Problem: Scenario Won't Load

**Symptoms**:
- "Failed to load scenario"
- Black screen on startup
- Crashes during load

**Solutions**:

1. **Clear cache**:
   ```bash
   python train_ai_atc.py --clear-cache
   ```

2. **Verify scenario files**:
   ```bash
   ls ~/.local/share/ai-atc/scenarios/
   ```

3. **Reset simulator**:
   ```bash
   python train_ai_atc.py --reset-all
   ```

4. **Check disk space**:
   - Ensure 1+ GB free
   - Delete old recordings if needed

## Scoring Issues

### Problem: Score Lower Than Expected

**Symptoms**:
- Score seems unfair
- Similar performance, different scores
- Don't understand score calculation

**Solutions**:

1. **Review [Scoring System](SCORING_SYSTEM.md)**

2. **Check penalties**:
   - Separation violations: -5 each
   - Altitude violations: -3 each
   - Speed violations: -2 each
   - Go-arounds: -3 each

3. **Calculate expected score**:
   - Base: 80 points
   - Minus 5 for each violation
   - Plus efficiency bonuses

4. **Example**:
   ```
   Perfect landing: 100 points
   1 separation warning: 95 points
   Efficient approach: 95 + 5 (bonus) = 100 points
   ```

### Problem: Leaderboard Score Different

**Symptoms**:
- Personal score: 80
- Leaderboard shows: 112
- Scores don't match

**Explanation**:
- Leaderboard applies difficulty multiplier
- Expert (1.6×): 80 × 1.6 = 128
- Advanced (1.4×): 80 × 1.4 = 112

**This is normal!**

## Data and Save Issues

### Problem: Progress Not Saved

**Symptoms**:
- Scores disappear
- Completed scenarios reset
- Settings lost

**Solutions**:

1. **Check save location**:
   ```bash
   # Linux/macOS
   ls ~/.local/share/ai-atc/

   # Windows
   dir %APPDATA%\ai-atc\
   ```

2. **Verify permissions**:
   ```bash
   chmod 755 ~/.local/share/ai-atc/
   ```

3. **Manual backup**:
   ```bash
   cp -r ~/.local/share/ai-atc/ ~/ai-atc-backup/
   ```

4. **Restore from backup**:
   ```bash
   cp -r ~/ai-atc-backup/* ~/.local/share/ai-atc/
   ```

### Problem: Can't Export Recording

**Symptoms**:
- Export fails
- Video file not created
- "Export failed" message

**Solutions**:

1. **Check ffmpeg installed**:
   ```bash
   ffmpeg -version
   ```

2. **Install ffmpeg**:
   - Windows: `choco install ffmpeg`
   - macOS: `brew install ffmpeg`
   - Linux: `sudo apt install ffmpeg`

3. **Check disk space**:
   - Export requires extra space
   - Ensure 2+ GB free

4. **Try different format**:
   ```bash
   python train_ai_atc.py --export-format mp4
   ```

## Getting Help

### Where to Report Issues

1. **GitHub Issues**: https://github.com/yourusername/AI-ATC/issues
2. **Discord Community**: (link)
3. **Email Support**: support@example.com

### Information to Include

When reporting a problem, include:
1. **System info**: OS, Python version, hardware
2. **Error message**: Exact text shown
3. **Steps to reproduce**: How to trigger issue
4. **Logs**: Console output (if applicable)

### Run Diagnostics

```bash
# Generate diagnostic report
python train_ai_atc.py --diagnostics

# This creates: ai-atc-diagnostics.txt
# Include this when reporting issues
```

## See Also

- [Getting Started](GETTING_STARTED.md) — Installation help
- [FAQ](FAQ.md) — Common questions
- [Best Practices](BEST_PRACTICES.md) — Performance tips
- [Keyboard Shortcuts](KEYBOARD_SHORTCUTS.md) — Shortcut reference

---

**Still stuck?** Open an issue on GitHub with diagnostic information, and the community will help! 🤝
