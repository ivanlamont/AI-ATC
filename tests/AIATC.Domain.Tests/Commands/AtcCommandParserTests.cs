using Xunit;
using AIATC.Domain.Models.Commands;

namespace AIATC.Domain.Tests.Commands;

public class AtcCommandParserTests
{
    private readonly AtcCommandParser _parser = new();

    [Theory]
    [InlineData("turn left heading 220", 220, TurnDirection.Left)]
    [InlineData("turn right heading 090", 90, TurnDirection.Right)]
    [InlineData("left 180", 180, TurnDirection.Left)]
    [InlineData("right 270", 270, TurnDirection.Right)]
    [InlineData("fly heading 360", 360, TurnDirection.Either)]
    [InlineData("heading 045", 45, TurnDirection.Either)]
    [InlineData("turn left 135", 135, TurnDirection.Left)]
    public void Parse_HeadingCommands_ReturnsCorrectHeading(
        string command, float expectedHeading, TurnDirection expectedDirection)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<HeadingCommand>(result);

        var headingCmd = (HeadingCommand)result;
        Assert.Equal(expectedHeading, headingCmd.TargetHeadingDegrees);
        Assert.Equal(expectedDirection, headingCmd.Direction);
    }

    [Theory]
    [InlineData("descend and maintain 4000", 4000, AltitudeChange.Descend)]
    [InlineData("climb and maintain 10000", 10000, AltitudeChange.Climb)]
    [InlineData("descend 3000", 3000, AltitudeChange.Descend)]
    [InlineData("climb 8000", 8000, AltitudeChange.Climb)]
    [InlineData("maintain 5000", 5000, AltitudeChange.Maintain)]
    [InlineData("maintain altitude 7000", 7000, AltitudeChange.Maintain)]
    [InlineData("5000 feet", 5000, AltitudeChange.Maintain)]
    public void Parse_AltitudeCommands_ReturnsCorrectAltitude(
        string command, float expectedAltitude, AltitudeChange expectedChange)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<AltitudeCommand>(result);

        var altCmd = (AltitudeCommand)result;
        Assert.Equal(expectedAltitude, altCmd.TargetAltitudeFeet);
        Assert.Equal(expectedChange, altCmd.ChangeType);
    }

    [Theory]
    [InlineData("reduce speed 180", 180, SpeedChange.Reduce)]
    [InlineData("increase speed 250", 250, SpeedChange.Increase)]
    [InlineData("maintain 220 knots", 220, SpeedChange.Maintain)]
    [InlineData("maintain speed 200", 200, SpeedChange.Maintain)]
    [InlineData("speed 190", 190, SpeedChange.Maintain)]
    [InlineData("slow 160", 160, SpeedChange.Reduce)]
    [InlineData("decrease speed to 170", 170, SpeedChange.Reduce)]
    public void Parse_SpeedCommands_ReturnsCorrectSpeed(
        string command, float expectedSpeed, SpeedChange expectedChange)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<SpeedCommand>(result);

        var speedCmd = (SpeedCommand)result;
        Assert.Equal(expectedSpeed, speedCmd.TargetSpeedKnots);
        Assert.Equal(expectedChange, speedCmd.ChangeType);
    }

    [Theory]
    [InlineData("proceed direct BEBOP", "BEBOP")]
    [InlineData("direct SUNST", "SUNST")]
    [InlineData("cleared direct to DUMBA", "DUMBA")]
    [InlineData("proceed direct bebop", "BEBOP")] // Should uppercase
    public void Parse_DirectCommands_ReturnsCorrectFix(string command, string expectedFix)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<DirectCommand>(result);

        var directCmd = (DirectCommand)result;
        Assert.Equal(expectedFix, directCmd.FixName);
    }

    [Theory]
    [InlineData("contact tower 120.5", "tower", 120.5f)]
    [InlineData("contact norcal approach 135.1", "norcal approach", 135.1f)]
    [InlineData("contact center 134.15", "center", 134.15f)]
    [InlineData("contact san francisco tower 120.5", "san francisco tower", 120.5f)]
    public void Parse_ContactCommands_ReturnsCorrectFacilityAndFrequency(
        string command, string expectedFacility, float expectedFrequency)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<ContactCommand>(result);

        var contactCmd = (ContactCommand)result;
        Assert.Equal(expectedFacility, contactCmd.FacilityName);
        Assert.Equal(expectedFrequency, contactCmd.FrequencyMhz, 2);
    }

    [Theory]
    [InlineData("cleared ILS runway 27", ApproachType.ILS, "27")]
    [InlineData("cleared visual approach runway 09", ApproachType.Visual, "09")]
    [InlineData("cleared RNAV 16L", ApproachType.RNAV, "16L")]
    [InlineData("cleared localizer runway 27R", ApproachType.Localizer, "27R")]
    [InlineData("cleared ILS 09L", ApproachType.ILS, "09L")]
    public void Parse_ApproachCommands_ReturnsCorrectTypeAndRunway(
        string command, ApproachType expectedType, string expectedRunway)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<ApproachCommand>(result);

        var approachCmd = (ApproachCommand)result;
        Assert.Equal(expectedType, approachCmd.Type);
        Assert.Equal(expectedRunway, approachCmd.RunwayIdentifier);
    }

    [Theory]
    [InlineData("hold at SUNST", "SUNST", null, TurnDirection.Right)]
    [InlineData("hold at BEBOP right turns", "BEBOP", null, TurnDirection.Right)]
    [InlineData("hold at DUMBA left turns", "DUMBA", null, TurnDirection.Left)]
    [InlineData("hold at SUNST 270 inbound right turns", "SUNST", 270f, TurnDirection.Right)]
    public void Parse_HoldCommands_ReturnsCorrectParameters(
        string command, string expectedFix, float? expectedCourse, TurnDirection expectedDirection)
    {
        var result = _parser.Parse(command);

        Assert.NotNull(result);
        Assert.IsType<HoldCommand>(result);

        var holdCmd = (HoldCommand)result;
        Assert.Equal(expectedFix, holdCmd.FixName);
        Assert.Equal(expectedCourse, holdCmd.InboundCourseDegrees);
        Assert.Equal(expectedDirection, holdCmd.TurnDirection);
    }

    [Theory]
    [InlineData("turn left heading two two zero", "turn left heading 220")]
    [InlineData("descend and maintain four thousand", "descend and maintain 4000")]
    [InlineData("heading zero niner zero", "heading 090")]
    public void Parse_HandlesWordNumbers(string command, string equivalent)
    {
        var result1 = _parser.Parse(command);
        var result2 = _parser.Parse(equivalent);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.GetType(), result2.GetType());
    }

    [Fact]
    public void ParseMultiple_HandlesMultipleCommands()
    {
        var commands = _parser.ParseMultiple("turn left heading 220 and descend 4000");

        Assert.Equal(2, commands.Count);
        Assert.IsType<HeadingCommand>(commands[0]);
        Assert.IsType<AltitudeCommand>(commands[1]);
    }

    [Fact]
    public void ParseMultiple_HandlesCommas()
    {
        var commands = _parser.ParseMultiple("heading 180, maintain 5000, speed 200");

        Assert.Equal(3, commands.Count);
        Assert.IsType<HeadingCommand>(commands[0]);
        Assert.IsType<AltitudeCommand>(commands[1]);
        Assert.IsType<SpeedCommand>(commands[2]);
    }

    [Fact]
    public void Parse_InvalidCommand_ReturnsNull()
    {
        var result = _parser.Parse("invalid command text");
        Assert.Null(result);
    }

    [Fact]
    public void CanParse_ValidCommand_ReturnsTrue()
    {
        Assert.True(_parser.CanParse("turn left heading 220"));
        Assert.True(_parser.CanParse("descend 4000"));
        Assert.True(_parser.CanParse("proceed direct BEBOP"));
    }

    [Fact]
    public void CanParse_InvalidCommand_ReturnsFalse()
    {
        Assert.False(_parser.CanParse("this is not a valid command"));
        Assert.False(_parser.CanParse(""));
    }

    [Fact]
    public void GetReadback_FormatsCorrectly()
    {
        var headingCmd = new HeadingCommand
        {
            TargetHeadingDegrees = 220,
            Direction = TurnDirection.Left
        };
        Assert.Contains("left", headingCmd.GetReadback().ToLower());
        Assert.Contains("220", headingCmd.GetReadback());

        var altCmd = new AltitudeCommand
        {
            TargetAltitudeFeet = 4000,
            ChangeType = AltitudeChange.Descend
        };
        Assert.Contains("descend", altCmd.GetReadback().ToLower());
        Assert.Contains("4", altCmd.GetReadback());
    }

    [Fact]
    public void GetSuggestions_ReturnsNonEmptyList()
    {
        var suggestions = _parser.GetSuggestions("turn left");
        Assert.NotEmpty(suggestions);

        suggestions = _parser.GetSuggestions("descend");
        Assert.NotEmpty(suggestions);

        suggestions = _parser.GetSuggestions("invalid command");
        Assert.NotEmpty(suggestions);
    }
}
