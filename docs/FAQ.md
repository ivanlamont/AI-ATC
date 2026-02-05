# ❓ Frequently Asked Questions (FAQ)

Quick answers to common questions about AI-ATC.

## Getting Started

### Q: How do I install AI-ATC?
**A:** See [Getting Started](GETTING_STARTED.md) for detailed installation instructions for Windows, macOS, or Linux.

### Q: What are the system requirements?
**A:**
- **CPU**: Intel Core i5 or equivalent
- **RAM**: 8GB minimum (16GB recommended)
- **Storage**: 2GB free space
- **GPU**: Optional (RTX series recommended)
- **Python**: 3.11 or later

### Q: How long does it take to learn?
**A:**
- **Basics**: 1-2 hours
- **Intermediate**: 1 week of practice
- **Advanced**: 2-3 weeks of practice
- **Expert**: 1-2 months of dedicated practice

### Q: Is there a tutorial?
**A:** Yes! Start with:
1. [Getting Started](GETTING_STARTED.md) guide
2. [Scenarios](SCENARIOS.md) - Beginner scenarios
3. [Best Practices](BEST_PRACTICES.md) for advanced techniques

## Commands and Operations

### Q: What's the correct syntax for commands?
**A:** Basic syntax: `<CALLSIGN> <INSTRUCTION> <PARAMETER>`

Example: `AAL456 descend to 3000`

See [ATC Command Reference](ATC_COMMAND_REFERENCE.md) for complete documentation.

### Q: Can I use abbreviations in commands?
**A:** Yes! Common abbreviations:
- **FT** = Feet
- **FL** = Flight Level
- **KTS** = Knots
- **HDG** = Heading
- **ALT** = Altitude

Example: `AAL456 descend FL200` = `AAL456 descend to flight level 200`

### Q: How do I issue multiple commands at once?
**A:** Use "and" to combine commands:
```
AAL456 descend to 2000 and turn right heading 270
```

This issues both commands simultaneously.

### Q: What if aircraft doesn't respond to my command?
**A:** Check:
1. **Callsign spelling**: Must match exactly (case-sensitive)
2. **Aircraft in range**: Must be in your airspace
3. **Command valid**: Check [ATC Command Reference](ATC_COMMAND_REFERENCE.md)
4. **Aircraft available**: Not already executing another command

### Q: Can I take back a command?
**A:** No, commands are permanent once issued. Use "go around" to abort landing, or issue correcting commands.

### Q: How do I handle an emergency?
**A:**
1. Identify emergency aircraft
2. Issue: `<CALLSIGN> go around, climb to safe altitude`
3. Clear other aircraft: Issue go-arounds as needed
4. Sequence emergency aircraft for landing
5. Issue final clearance when ready

Example in [Best Practices](BEST_PRACTICES.md#emergency-response).

## Scenarios and Difficulty

### Q: How do I choose a difficulty level?
**A:**
- **Beginner**: Learning the basics
- **Intermediate**: Comfortable with commands, want more challenge
- **Advanced**: Managing multiple aircraft easily
- **Expert**: Seeking maximum challenge

Progress through levels in order for best learning.

### Q: What's the difference between scenarios?
**A:** Scenarios vary by:
- **Aircraft count** (1-8+)
- **Aircraft mix** (VFR/IFR)
- **Weather** (calm to severe)
- **Runway configuration** (single or multiple)

See [Scenarios](SCENARIOS.md) for complete descriptions.

### Q: Can I create custom scenarios?
**A:** Yes! Use the custom scenario builder:
```
python train_ai_atc.py --custom --aircraft 4 --wind 15 --difficulty advanced
```

See command-line help for all options.

### Q: How long does each scenario take?
**A:**
- **Beginner**: 3-10 minutes
- **Intermediate**: 15-25 minutes
- **Advanced**: 25-35 minutes
- **Expert**: 30-45+ minutes

Times vary based on your efficiency.

## Scoring and Performance

### Q: How is my score calculated?
**A:** Score = Safety (40 pts) + Efficiency (40 pts) + Bonuses (0-20 pts) - Penalties

Detailed breakdown: [Scoring System](SCORING_SYSTEM.md)

### Q: What's a good score?
**A:**
- **60-70**: Acceptable (C-Rank)
- **70-80**: Good (B-Rank)
- **80-90**: Very good (A-Rank)
- **90+**: Excellent (S-Rank)

Leaderboard rankings adjust by scenario difficulty.

### Q: How do separation violations affect my score?
**A:** Each violation: -5 points
- **Horizontal violation** (< 2 nm at same altitude): -5 points
- **Vertical violation** (< 1,000 ft): -5 points
- **Multiple violations**: -10 to -25 points depending on severity

### Q: Can I improve my previous score?
**A:** Yes! Replay scenarios:
```
python train_ai_atc.py --scenario-id <ID> --retry
```

Scores on leaderboard are personal records.

### Q: What are bonuses?
**A:** Special achievements worth 5-25 points:
- Weather challenges
- High-density traffic
- Emergency handling
- Perfect execution
- Speed completion

See [Scoring System](SCORING_SYSTEM.md#bonus-points) for details.

## Wind and Weather

### Q: How does wind affect approaches?
**A:**
- **Headwind** (good): Helps deceleration, shorter landing distance
- **Tailwind** (bad): Longer landing distance, may exceed runway limits
- **Crosswind** (varies): Each aircraft has maximum (typically 15 knots)

### Q: How do I calculate headwind/crosswind?
**A:**
```
Headwind = Wind Speed × cos(Runway - Wind Direction)
Crosswind = Wind Speed × sin(Runway - Wind Direction)
```

Example: Wind 20 kts from 180°, Runway 09 (090°):
- Headwind = 20 × cos(090-180) = 20 × cos(-90) ≈ 0 kts
- Crosswind = 20 × sin(-90) ≈ -20 kts (20 kts from left)

### Q: When should I change runways?
**A:** Change when:
- Wind shift >30° from current runway
- Crosswind exceeds aircraft limits
- Headwind becomes strong tailwind
- Better runway available for new traffic

### Q: How often can I change runways?
**A:** Minimum 5 minutes between changes (operational requirement)

### Q: How does weather affect score?
**A:** Severe weather scenarios:
- Lower base difficulty
- +10 bonus points for completion
- More credit for safety

## Aircraft Management

### Q: What's the difference between VFR and IFR?
**A:**
- **VFR** (Visual Flight Rules):
  - Lower speeds (80-150 knots)
  - Visual navigation
  - Require flight following
  - Max altitude ~10,000 ft

- **IFR** (Instrument Flight Rules):
  - Higher speeds (150-400 knots)
  - Radar navigation
  - Instrument approach
  - Can fly higher altitudes

### Q: How do I handle VFR aircraft?
**A:**
1. Use flight following service
2. Provide traffic advisories
3. Lower speed management
4. Visual approach patterns
5. Larger separation buffer (2 nm)

### Q: Why is one aircraft so slow?
**A:** Likely a VFR aircraft (Cessna, Piper, etc.)
- Typical speed: 100-120 knots
- Can't fly as high as commercial jets
- Requires different approach procedures

See [VFR Support](GETTING_STARTED.md) section for details.

### Q: Can I merge fast and slow aircraft?
**A:** Yes, with proper planning:
1. Slow aircraft gets early descent
2. Fast aircraft delayed descent
3. Fast aircraft reduces speed on approach
4. Proper separation maintained throughout

See [Best Practices](BEST_PRACTICES.md#speed-based-sequencing) for technique.

## Separation and Safety

### Q: What's the minimum separation?
**A:**
- **Horizontal**: 2 nautical miles (same altitude)
- **Vertical**: 1,000 feet
- **VFR/IFR**: 2 nautical miles (different rules)

### Q: How do I avoid separation violations?
**A:**
1. Monitor all aircraft positions
2. Plan ahead for conflicts
3. Use altitude or speed for separation
4. Build in safety margins
5. Issue go-arounds if uncertain

### Q: What's wake turbulence?
**A:** Disturbed air behind large aircraft that affects smaller aircraft

**Rules**:
- Heavy aircraft → Light aircraft: 3 minutes or 5 nm
- Heavy → Medium: 2 minutes or 3 nm
- Light → Light: Standard separation

### Q: When should I issue a go-around?
**A:** Issue when:
- Traffic conflict ahead
- Unstable approach
- Wind exceeds limits
- Runway blocked/unsafe
- Pilot requests
- Any doubt about safety

Safety first, always!

## Performance and Optimization

### Q: My game is running slowly. What can I do?
**A:**
1. Close other applications
2. Reduce visual quality settings
3. Lower aircraft count
4. Check system requirements
5. Update GPU drivers

See [Troubleshooting](TROUBLESHOOTING.md) for more solutions.

### Q: How can I improve my score?
**A:**
1. **Study**: Read [Best Practices](BEST_PRACTICES.md)
2. **Practice**: Repeat scenarios at your level
3. **Focus**: Improve specific weaknesses
4. **Learn**: Analyze your performance metrics
5. **Progress**: Move to harder difficulties

### Q: What's the highest possible score?
**A:**
- **Base**: 100 points
- **With bonuses**: 120+ points on leaderboard
- **With difficulty multiplier**: Up to 160 points (Expert × 1.6)

### Q: How do I beat my personal record?
**A:**
1. Replay the same scenario
2. Focus on safety (zero violations)
3. Optimize efficiency (faster landings)
4. Execute perfectly (minimal extra commands)
5. Collect bonuses (challenges)

## Leaderboards and Competition

### Q: How do leaderboards work?
**A:**
- **Monthly reset**: New season each month
- **Per-difficulty**: Separate rankings by difficulty
- **Personal bests**: Highest score per scenario
- **Multipliers**: Score adjusted by difficulty

### Q: Why is my score different on the leaderboard?
**A:**
- Score multiplied by difficulty factor
- Only personal best scores count
- Some bonuses award extra points

Example: 80 points on Expert (1.6×) = 128 points on leaderboard

### Q: Can I compete with others?
**A:** Yes!
- **Asynchronous**: Compare scores anytime
- **Challenge mode**: Head-to-head competition (if available)
- **Team scores**: Cooperative scenarios

### Q: How do I reset my score?
**A:** Scores don't reset per-scenario, but:
- Monthly leaderboards reset automatically
- Personal records preserved in history
- Replay scenarios to improve

## Keyboard Shortcuts

### Q: What are the keyboard shortcuts?
**A:**

| Key | Action |
|-----|--------|
| Space | Pause/Resume |
| +/- | Speed up/Slow down |
| R | Reset scenario |
| H | Help |
| Q | Quit |
| 1-5 | Change difficulty |

Complete list: [Keyboard Shortcuts](KEYBOARD_SHORTCUTS.md)

### Q: Can I customize keyboard shortcuts?
**A:** Yes! Edit `~/.claude/keybindings.json` or use settings menu.

### Q: How do I use autocomplete?
**A:**
- **Tab**: Complete callsign
- **Up Arrow**: Previous command history
- **Down Arrow**: Next command

## Technical Issues

### Q: Where are saved games stored?
**A:**
- **Linux/macOS**: `~/.local/share/ai-atc/`
- **Windows**: `%APPDATA%\ai-atc\`

### Q: How do I report a bug?
**A:**
1. Go to GitHub: https://github.com/yourusername/AI-ATC
2. Click "Issues"
3. Click "New Issue"
4. Describe the problem
5. Include system info and steps to reproduce

### Q: How do I reset the simulator?
**A:**
```bash
python train_ai_atc.py --reset-all
```

This clears saved data, scores, and settings.

### Q: Can I use this on a Mac?
**A:** Yes! Python is cross-platform. Follow [Getting Started](GETTING_STARTED.md) for setup.

### Q: Is there a mobile version?
**A:** Not currently, but may be planned for future releases.

## Advanced Topics

### Q: How do I train the AI model?
**A:**
```bash
python train_ai_atc.py --train --episodes 1000
```

See [Training Guide](../IMPLEMENTATION_ROADMAP.md) for details.

### Q: Can I modify scenarios?
**A:** Yes, using custom scenario files (see documentation).

### Q: How do I contribute to the project?
**A:**
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

See CONTRIBUTING.md for guidelines.

### Q: Where can I find the source code?
**A:** GitHub: https://github.com/yourusername/AI-ATC

## Getting Help

### Q: Where's the full documentation?
**A:** In the `docs/` folder:
- [Getting Started](GETTING_STARTED.md)
- [ATC Commands](ATC_COMMAND_REFERENCE.md)
- [Scenarios](SCENARIOS.md)
- [Scoring](SCORING_SYSTEM.md)
- [Best Practices](BEST_PRACTICES.md)
- [Troubleshooting](TROUBLESHOOTING.md)

### Q: Can I get live help?
**A:**
- **In-game**: Press `H` for help
- **Discord**: Join our community server
- **GitHub Issues**: Ask questions there

### Q: How do I suggest a feature?
**A:**
1. Check existing GitHub issues
2. Open a new issue with "Feature Request" label
3. Describe the feature
4. Explain the benefit

### Q: Is there a community forum?
**A:** Yes! Check GitHub Discussions for community chat.

## Troubleshooting

### Q: My aircraft won't land. What's wrong?
**A:** Check:
1. Runway alignment (within 5 degrees)
2. Altitude (below 500 ft)
3. Speed (within 10 knots of target)
4. Wind conditions (within limits)

See [Troubleshooting](TROUBLESHOOTING.md) for detailed solutions.

### Q: Commands aren't working. Why?
**A:**
1. Verify callsign spelling
2. Check command syntax
3. Ensure aircraft in controlled airspace
4. Confirm command is valid

### Q: I'm getting too many violations. Help!
**A:**
1. Reduce traffic density
2. Practice at easier difficulty
3. Study [Best Practices](BEST_PRACTICES.md)
4. Use larger separation margins

### Q: The simulation is too fast/slow. How do I adjust?
**A:**
- **Faster**: Press `+` key or use `--speed 2`
- **Slower**: Press `-` key or use `--speed 0.5`

## See Also

- [Getting Started](GETTING_STARTED.md) — New to AI-ATC?
- [Troubleshooting](TROUBLESHOOTING.md) — Technical problems
- [Keyboard Shortcuts](KEYBOARD_SHORTCUTS.md) — All shortcuts
- [Best Practices](BEST_PRACTICES.md) — Expert tips

---

**Still have questions?** Open an issue on GitHub or ask in the community Discord!
