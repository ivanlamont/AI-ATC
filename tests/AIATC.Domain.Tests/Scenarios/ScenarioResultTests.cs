using AIATC.Domain.Models.Scenarios;
using Xunit;

namespace AIATC.Domain.Tests.Scenarios;

public class ScenarioResultTests
{
    [Fact]
    public void CreateSuccess_NoViolations_ReturnsFiveStars()
    {
        var result = ScenarioResult.CreateSuccess(1200, 600, 10, 0);

        Assert.True(result.Success);
        Assert.Equal(5, result.StarRating);
        Assert.Equal("A+", result.Grade);
    }

    [Fact]
    public void CreateSuccess_OneViolation_ReturnsThreeStars()
    {
        var result = ScenarioResult.CreateSuccess(800, 600, 10, 1);

        Assert.True(result.Success);
        Assert.Equal(3, result.StarRating);
    }

    [Fact]
    public void CreateSuccess_ManyViolations_ReturnsOneStar()
    {
        var result = ScenarioResult.CreateSuccess(400, 600, 10, 5);

        Assert.True(result.Success);
        Assert.Equal(1, result.StarRating);
        Assert.Equal("F", result.Grade);
    }

    [Fact]
    public void CreateSuccess_HighScore_ReturnsAPlusGrade()
    {
        var result = ScenarioResult.CreateSuccess(1500, 600, 15, 0);

        Assert.Equal("A+", result.Grade);
    }

    [Fact]
    public void CreateSuccess_MediumScore_ReturnsAGrade()
    {
        var result = ScenarioResult.CreateSuccess(800, 600, 10, 0);

        Assert.Equal("A", result.Grade);
    }

    [Fact]
    public void CreateSuccess_LowScore_ReturnsBGrade()
    {
        var result = ScenarioResult.CreateSuccess(600, 600, 8, 0);

        Assert.Equal("B", result.Grade);
    }

    [Fact]
    public void CreateSuccess_GeneratesComments()
    {
        var result = ScenarioResult.CreateSuccess(1000, 1200, 10, 0);

        Assert.NotEmpty(result.Comments);
        Assert.Contains(result.Comments, c => c.Contains("10 aircraft"));
        Assert.Contains(result.Comments, c => c.Contains("Perfect separation"));
    }

    [Fact]
    public void CreateFailed_SetsFailureProperties()
    {
        var result = ScenarioResult.CreateFailed("Time limit exceeded");

        Assert.False(result.Success);
        Assert.Equal(0, result.StarRating);
        Assert.Equal("F", result.Grade);
        Assert.Equal("Time limit exceeded", result.FailureReason);
        Assert.Single(result.Comments);
    }

    [Fact]
    public void CreateSuccess_ManyAircraft_GeneratesPositiveComment()
    {
        var result = ScenarioResult.CreateSuccess(1500, 1800, 15, 0);

        Assert.Contains(result.Comments, c => c.Contains("Excellent work"));
    }

    [Fact]
    public void CreateSuccess_FewAircraft_GeneratesNeutralComment()
    {
        var result = ScenarioResult.CreateSuccess(500, 600, 3, 0);

        Assert.Contains(result.Comments, c => c.Contains("Landed 3 aircraft"));
    }
}
