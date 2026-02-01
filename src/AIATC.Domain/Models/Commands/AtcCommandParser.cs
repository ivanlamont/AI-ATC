using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace AIATC.Domain.Models.Commands;

/// <summary>
/// Parses natural language ATC commands into structured command objects.
/// Handles standard ATC phraseology with variations.
/// </summary>
public class AtcCommandParser
{
    private static readonly Dictionary<string, string> Replacements = new()
    {
        { "one", "1" }, { "two", "2" }, { "three", "3" }, { "four", "4" },
        { "five", "5" }, { "six", "6" }, { "seven", "7" }, { "eight", "8" },
        { "nine", "9" }, { "zero", "0" }, { "niner", "9" }
    };

    /// <summary>
    /// Parses a text command into a structured ATC command
    /// </summary>
    public AtcCommand? Parse(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return null;

        var normalized = NormalizeCommand(commandText);

        // Try to parse different command types in priority order
        AtcCommand? result = TryParseHeading(normalized);
        if (result != null) return result;

        result = TryParseAltitude(normalized);
        if (result != null) return result;

        result = TryParseSpeed(normalized);
        if (result != null) return result;

        result = TryParseDirect(normalized);
        if (result != null) return result;

        result = TryParseContact(normalized);
        if (result != null) return result;

        result = TryParseApproach(normalized);
        if (result != null) return result;

        return TryParseHold(normalized);
    }

    /// <summary>
    /// Parses multiple commands from a single text (separated by "and" or commas)
    /// </summary>
    public List<AtcCommand> ParseMultiple(string commandText)
    {
        var commands = new List<AtcCommand>();

        // Split on "and" or commas, but preserve numbers
        var parts = Regex.Split(commandText, @"\s+and\s+|,\s*", RegexOptions.IgnoreCase);

        foreach (var part in parts)
        {
            var cmd = Parse(part.Trim());
            if (cmd != null)
            {
                commands.Add(cmd);
            }
        }

        return commands;
    }

    private string NormalizeCommand(string text)
    {
        text = text.ToLowerInvariant().Trim();

        // Replace word numbers with digits
        foreach (var replacement in Replacements)
        {
            text = Regex.Replace(text, $@"\b{replacement.Key}\b", replacement.Value, RegexOptions.IgnoreCase);
        }

        // Normalize multiple spaces
        text = Regex.Replace(text, @"\s+", " ");

        return text;
    }

    private HeadingCommand? TryParseHeading(string text)
    {
        // Patterns: "turn left heading 220", "left 180", "fly heading 090", "turn right 270"
        var patterns = new[]
        {
            @"(?:turn\s+)?(left|right)\s+(?:heading\s+)?(\d{1,3})",
            @"(?:fly\s+)?heading\s+(\d{1,3})",
            @"turn\s+(\d{1,3})\s+(left|right)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                TurnDirection direction = TurnDirection.Either;
                float heading = 0;

                if (match.Groups.Count == 3)
                {
                    // Pattern with explicit direction
                    var dirText = match.Groups[1].Value.ToLower();
                    direction = dirText == "left" ? TurnDirection.Left : TurnDirection.Right;
                    heading = float.Parse(match.Groups[2].Value);
                }
                else if (match.Groups.Count == 2)
                {
                    // Pattern: "heading 270" or "turn 180 right"
                    heading = float.Parse(match.Groups[1].Value);

                    if (match.Groups.Count > 2 && match.Groups[2].Success)
                    {
                        var dirText = match.Groups[2].Value.ToLower();
                        direction = dirText == "left" ? TurnDirection.Left : TurnDirection.Right;
                    }
                }

                return new HeadingCommand
                {
                    TargetHeadingDegrees = heading,
                    Direction = direction,
                    OriginalText = text
                };
            }
        }

        return null;
    }

    private AltitudeCommand? TryParseAltitude(string text)
    {
        // Patterns: "descend and maintain 4000", "climb 10000", "maintain 5000"
        // Note: altitude values are typically >= 1000 feet to distinguish from speeds
        var patterns = new[]
        {
            @"(climb|descend)\s+(?:and\s+)?(?:maintain\s+)?(\d+)",
            @"maintain\s+altitude\s+(\d+)",
            @"maintain\s+(\d{4,})", // 4+ digits = altitude (e.g., 5000, not 220)
            @"(\d{4,})\s+feet?" // altitude in feet
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var changeType = AltitudeChange.Maintain;
                float altitude = 0;

                if (match.Groups.Count == 3)
                {
                    var action = match.Groups[1].Value.ToLower();
                    changeType = action == "climb" ? AltitudeChange.Climb : AltitudeChange.Descend;
                    altitude = float.Parse(match.Groups[2].Value);
                }
                else
                {
                    altitude = float.Parse(match.Groups[1].Value);
                }

                return new AltitudeCommand
                {
                    TargetAltitudeFeet = altitude,
                    ChangeType = changeType,
                    OriginalText = text
                };
            }
        }

        return null;
    }

    private SpeedCommand? TryParseSpeed(string text)
    {
        // Patterns: "reduce speed 180", "increase speed 250", "maintain 200 knots"
        var patterns = new[]
        {
            @"(reduce|decrease|slow|increase|speed\s+up)\s+(?:speed\s+)?(?:to\s+)?(\d+)",
            @"maintain\s+(?:speed\s+)?(\d+)",
            @"speed\s+(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var changeType = SpeedChange.Maintain;
                float speed = 0;

                if (match.Groups.Count == 3)
                {
                    var action = match.Groups[1].Value.ToLower();
                    changeType = (action.Contains("reduce") || action.Contains("slow") || action.Contains("decrease"))
                        ? SpeedChange.Reduce
                        : SpeedChange.Increase;
                    speed = float.Parse(match.Groups[2].Value);
                }
                else
                {
                    speed = float.Parse(match.Groups[1].Value);
                }

                return new SpeedCommand
                {
                    TargetSpeedKnots = speed,
                    ChangeType = changeType,
                    OriginalText = text
                };
            }
        }

        return null;
    }

    private DirectCommand? TryParseDirect(string text)
    {
        // Patterns: "proceed direct BEBOP", "direct DUMBA", "cleared direct to SUNST"
        var pattern = @"(?:proceed\s+)?(?:cleared\s+)?direct\s+(?:to\s+)?([A-Z]{3,5})";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return new DirectCommand
            {
                FixName = match.Groups[1].Value.ToUpperInvariant(),
                OriginalText = text
            };
        }

        return null;
    }

    private ContactCommand? TryParseContact(string text)
    {
        // Patterns: "contact tower 120.5", "contact norcal approach 135.1"
        var pattern = @"contact\s+([a-z\s]+?)\s+(\d{3}\.?\d{1,2})";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var facility = match.Groups[1].Value.Trim();
            var freqText = match.Groups[2].Value;

            // Parse frequency (handle both "120.5" and "1205")
            float frequency;
            if (freqText.Contains('.'))
            {
                frequency = float.Parse(freqText);
            }
            else
            {
                // Convert "1205" to "120.5"
                frequency = float.Parse(freqText) / 10.0f;
            }

            return new ContactCommand
            {
                FacilityName = facility,
                FrequencyMhz = frequency,
                OriginalText = text
            };
        }

        return null;
    }

    private ApproachCommand? TryParseApproach(string text)
    {
        // Patterns: "cleared ILS runway 27", "cleared visual approach runway 09", "cleared RNAV 16L"
        var pattern = @"cleared\s+(ils|visual|rnav|localizer|vor|ndb)?\s*(?:approach\s+)?(?:runway\s+)?(\d{1,2}[LRC]?)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var typeText = match.Groups[1].Success ? match.Groups[1].Value.ToLower() : "ils";
            var runway = match.Groups[2].Value.ToUpperInvariant();

            var type = typeText switch
            {
                "ils" => ApproachType.ILS,
                "visual" => ApproachType.Visual,
                "rnav" => ApproachType.RNAV,
                "localizer" => ApproachType.Localizer,
                "vor" => ApproachType.VOR,
                "ndb" => ApproachType.NDB,
                _ => ApproachType.ILS
            };

            return new ApproachCommand
            {
                Type = type,
                RunwayIdentifier = runway,
                OriginalText = text
            };
        }

        return null;
    }

    private HoldCommand? TryParseHold(string text)
    {
        // Patterns: "hold at SUNST", "hold at BEBOP 270 inbound right turns"
        var pattern = @"hold\s+(?:at\s+)?([A-Z]{3,5})(?:\s+(\d{3})?\s+inbound)?(?:\s+(left|right)\s+turns)?";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var fix = match.Groups[1].Value.ToUpperInvariant();
            float? course = null;
            var direction = TurnDirection.Right;

            if (match.Groups[2].Success)
            {
                course = float.Parse(match.Groups[2].Value);
            }

            if (match.Groups[3].Success)
            {
                direction = match.Groups[3].Value.ToLower() == "left"
                    ? TurnDirection.Left
                    : TurnDirection.Right;
            }

            return new HoldCommand
            {
                FixName = fix,
                InboundCourseDegrees = course,
                TurnDirection = direction,
                OriginalText = text
            };
        }

        return null;
    }

    /// <summary>
    /// Validates if a command text is parseable
    /// </summary>
    public bool CanParse(string commandText)
    {
        return Parse(commandText) != null;
    }

    /// <summary>
    /// Gets suggested corrections for unparseable commands
    /// </summary>
    public List<string> GetSuggestions(string commandText)
    {
        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(commandText))
            return suggestions;

        var lower = commandText.ToLowerInvariant();

        // Check for common mistakes and suggest corrections
        if (lower.Contains("heading") && !Regex.IsMatch(lower, @"\d{1,3}"))
        {
            suggestions.Add("Try: 'turn left heading 220' or 'heading 090'");
        }

        if (lower.Contains("altitude") || lower.Contains("climb") || lower.Contains("descend"))
        {
            if (!Regex.IsMatch(lower, @"\d{3,5}"))
            {
                suggestions.Add("Try: 'descend and maintain 4000' or 'climb 10000'");
            }
        }

        if (lower.Contains("speed") && !Regex.IsMatch(lower, @"\d{2,3}"))
        {
            suggestions.Add("Try: 'reduce speed 180' or 'maintain 220 knots'");
        }

        if (lower.Contains("direct") && !Regex.IsMatch(lower, @"[A-Z]{3,5}"))
        {
            suggestions.Add("Try: 'proceed direct BEBOP' (use 5-letter fix name)");
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("Command not recognized. Use standard ATC phraseology.");
        }

        return suggestions;
    }
}
