using AIATC.Domain.Models.Weather;
using Xunit;

namespace AIATC.Domain.Tests.Weather;

public class VisibilityConditionsTests
{
    [Fact]
    public void IsVmc_GoodVisibilityHighAltitude_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 10 };
        Assert.True(vis.IsVmc(15000));
    }

    [Fact]
    public void IsVmc_PoorVisibilityHighAltitude_ReturnsFalse()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 3 };
        Assert.False(vis.IsVmc(15000));
    }

    [Fact]
    public void IsVmc_GoodVisibilityLowAltitude_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 5 };
        Assert.True(vis.IsVmc(5000));
    }

    [Fact]
    public void IsIfr_LowVisibility_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 2 };
        Assert.True(vis.IsIfr(null));
    }

    [Fact]
    public void IsIfr_LowCeiling_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 5 };
        Assert.True(vis.IsIfr(800));
    }

    [Fact]
    public void IsIfr_GoodConditions_ReturnsFalse()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 10 };
        Assert.False(vis.IsIfr(3500));
    }

    [Fact]
    public void IsLifr_VeryLowVisibility_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 0.5f };
        Assert.True(vis.IsLifr(null));
    }

    [Fact]
    public void IsLifr_VeryLowCeiling_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 3 };
        Assert.True(vis.IsLifr(400));
    }

    [Fact]
    public void IsMvfr_MarginalCeiling_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 10 };
        Assert.True(vis.IsMvfr(2000));
    }

    [Fact]
    public void IsMvfr_MarginalVisibility_ReturnsTrue()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 4 };
        Assert.True(vis.IsMvfr(5000));
    }

    [Fact]
    public void GetFlightCategory_Clear_ReturnsVfr()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 10 };
        Assert.Equal("VFR", vis.GetFlightCategory(5000));
    }

    [Fact]
    public void GetFlightCategory_Marginal_ReturnsMvfr()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 4 };
        Assert.Equal("MVFR", vis.GetFlightCategory(5000));
    }

    [Fact]
    public void GetFlightCategory_Instrument_ReturnsIfr()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 2 };
        Assert.Equal("IFR", vis.GetFlightCategory(1500));
    }

    [Fact]
    public void GetFlightCategory_Low_ReturnsLifr()
    {
        var vis = new VisibilityConditions { VisibilityMiles = 0.5f };
        Assert.Equal("LIFR", vis.GetFlightCategory(300));
    }

    [Fact]
    public void ToMetarString_Clear_FormatsCorrectly()
    {
        var vis = VisibilityConditions.CreateClear();
        Assert.Equal("10SM", vis.ToMetarString());
    }

    [Fact]
    public void ToMetarString_LightRain_FormatsCorrectly()
    {
        var vis = new VisibilityConditions
        {
            VisibilityMiles = 5,
            Precipitation = PrecipitationType.Rain,
            Intensity = PrecipitationIntensity.Light
        };

        Assert.Equal("5SM -RA", vis.ToMetarString());
    }

    [Fact]
    public void ToMetarString_HeavyRainWithFog_FormatsCorrectly()
    {
        var vis = new VisibilityConditions
        {
            VisibilityMiles = 1,
            Precipitation = PrecipitationType.Rain,
            Intensity = PrecipitationIntensity.Heavy,
            Obscuration = ObscurationType.Fog
        };

        Assert.Equal("1SM +RAFG", vis.ToMetarString());
    }

    [Fact]
    public void CreateClear_ReturnsGoodConditions()
    {
        var vis = VisibilityConditions.CreateClear();

        Assert.Equal(10f, vis.VisibilityMiles);
        Assert.Equal(PrecipitationType.None, vis.Precipitation);
        Assert.Equal(ObscurationType.None, vis.Obscuration);
    }

    [Fact]
    public void CreateIfr_ReturnsIfrConditions()
    {
        var vis = VisibilityConditions.CreateIfr();

        Assert.Equal(2f, vis.VisibilityMiles);
        Assert.Equal(PrecipitationType.Rain, vis.Precipitation);
        Assert.Equal(ObscurationType.Mist, vis.Obscuration);
    }
}
