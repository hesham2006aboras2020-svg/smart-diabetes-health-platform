using Domain.Enums;
using Domain.Rules;

namespace Tests;

public sealed class GlucoseRulesTests
{
    [Fact]
    public void ToMgDl_ConvertsMmolL()
    {
        var result = GlucoseValueConverter.ToMgDl(5m, GlucoseUnit.MmolL);
        Assert.Equal(90m, result);
    }

    [Fact]
    public void ToMgDl_KeepsMgDlUnchanged()
    {
        var result = GlucoseValueConverter.ToMgDl(120m, GlucoseUnit.MgDl);
        Assert.Equal(120m, result);
    }

    [Fact]
    public void ThresholdsMatchProjectRules()
    {
        Assert.Equal(70m, GlucoseRules.LowThresholdMgDl);
        Assert.Equal(180m, GlucoseRules.HighThresholdMgDl);
        Assert.Equal(10m, GlucoseRules.TrendStabilityDeltaMgDl);
    }
}