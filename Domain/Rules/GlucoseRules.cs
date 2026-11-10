namespace Domain.Rules;

public static class GlucoseRules
{
    public const decimal LowThresholdMgDl = 70m;
    public const decimal HighThresholdMgDl = 180m;
    public const decimal TrendStabilityDeltaMgDl = 10m;
}