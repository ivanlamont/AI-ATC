using AIATC.Domain.Models.Weather;
using Xunit;

namespace AIATC.Domain.Tests.Weather;

public class CloudLayerTests
{
    [Fact]
    public void ContainsAltitude_WithinLayer_ReturnsTrue()
    {
        var cloud = new CloudLayer
        {
            BaseAltitudeAgl = 1000,
            TopAltitudeAgl = 3000
        };

        Assert.True(cloud.ContainsAltitude(1000));
        Assert.True(cloud.ContainsAltitude(2000));
        Assert.True(cloud.ContainsAltitude(3000));
    }

    [Fact]
    public void ContainsAltitude_OutsideLayer_ReturnsFalse()
    {
        var cloud = new CloudLayer
        {
            BaseAltitudeAgl = 1000,
            TopAltitudeAgl = 3000
        };

        Assert.False(cloud.ContainsAltitude(999));
        Assert.False(cloud.ContainsAltitude(3001));
    }

    [Fact]
    public void IsCeiling_BrokenClouds_ReturnsTrue()
    {
        var cloud = new CloudLayer { Coverage = CloudCoverage.Broken };
        Assert.True(cloud.IsCeiling());
    }

    [Fact]
    public void IsCeiling_OvercastClouds_ReturnsTrue()
    {
        var cloud = new CloudLayer { Coverage = CloudCoverage.Overcast };
        Assert.True(cloud.IsCeiling());
    }

    [Fact]
    public void IsCeiling_ScatteredClouds_ReturnsFalse()
    {
        var cloud = new CloudLayer { Coverage = CloudCoverage.Scattered };
        Assert.False(cloud.IsCeiling());
    }

    [Fact]
    public void IsCeiling_FewClouds_ReturnsFalse()
    {
        var cloud = new CloudLayer { Coverage = CloudCoverage.Few };
        Assert.False(cloud.IsCeiling());
    }

    [Fact]
    public void ToMetarString_Clear_ReturnsCorrectFormat()
    {
        var cloud = new CloudLayer { Coverage = CloudCoverage.Clear };
        Assert.Equal("CLR", cloud.ToMetarString());
    }

    [Fact]
    public void ToMetarString_ScatteredAt2500_ReturnsCorrectFormat()
    {
        var cloud = new CloudLayer
        {
            Coverage = CloudCoverage.Scattered,
            BaseAltitudeAgl = 2500
        };

        Assert.Equal("SCT025", cloud.ToMetarString());
    }

    [Fact]
    public void ToMetarString_BrokenAt1000_ReturnsCorrectFormat()
    {
        var cloud = new CloudLayer
        {
            Coverage = CloudCoverage.Broken,
            BaseAltitudeAgl = 1000
        };

        Assert.Equal("BKN010", cloud.ToMetarString());
    }

    [Fact]
    public void ToMetarString_OvercastAt500_ReturnsCorrectFormat()
    {
        var cloud = new CloudLayer
        {
            Coverage = CloudCoverage.Overcast,
            BaseAltitudeAgl = 500
        };

        Assert.Equal("OVC005", cloud.ToMetarString());
    }

    [Fact]
    public void CreateCeiling_SetsCorrectParameters()
    {
        var cloud = CloudLayer.CreateCeiling(1200);

        Assert.Equal(CloudCoverage.Overcast, cloud.Coverage);
        Assert.Equal(1200f, cloud.BaseAltitudeAgl);
        Assert.Equal(3200f, cloud.TopAltitudeAgl);
        Assert.Equal(CloudType.Stratus, cloud.Type);
    }

    [Fact]
    public void CreateScattered_SetsCorrectParameters()
    {
        var cloud = CloudLayer.CreateScattered(3500, 1500);

        Assert.Equal(CloudCoverage.Scattered, cloud.Coverage);
        Assert.Equal(3500f, cloud.BaseAltitudeAgl);
        Assert.Equal(5000f, cloud.TopAltitudeAgl);
        Assert.Equal(CloudType.Cumulus, cloud.Type);
    }
}
