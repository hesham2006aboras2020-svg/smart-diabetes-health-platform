namespace Domain.Rules;

public readonly record struct StabilityReading(decimal ValueMgDl, DateTime MeasurementDateTime);

public static class StabilityScoreCalculator
{
    public const int MinimumReadings = 7;
    public const int MinimumDistinctMeasurementDays = 3;

    public static decimal? Calculate(IReadOnlyList<StabilityReading> readings)
    {
        if (!HasSufficientData(readings))
            return null;

        var inRange = readings.Count(x => x.ValueMgDl is >= GlucoseRules.LowThresholdMgDl and <= GlucoseRules.HighThresholdMgDl) * 100m / readings.Count;
        var high = readings.Count(x => x.ValueMgDl > GlucoseRules.HighThresholdMgDl) * 100m / readings.Count;
        var low = readings.Count(x => x.ValueMgDl < GlucoseRules.LowThresholdMgDl) * 100m / readings.Count;

        var trend = CalculateTrend(readings.Select(x => x.ValueMgDl).ToList());
        var penalty = Math.Min(40m, (high + low) * 0.8m);
        var trendPenalty = trend == Enums.TrendType.Worsening ? 10m : trend == Enums.TrendType.Improving ? 5m : 0m;
        var dataBonus = 10m;

        return Math.Round(Math.Clamp(inRange - penalty + dataBonus - trendPenalty, 0m, 100m), 2);
    }

    public static bool HasSufficientData(IReadOnlyList<StabilityReading> readings) =>
        readings.Count >= MinimumReadings &&
        readings.Select(x => x.MeasurementDateTime.Date).Distinct().Count() >= MinimumDistinctMeasurementDays;

    private static Enums.TrendType CalculateTrend(IReadOnlyList<decimal> values)
    {
        if (values.Count < 4) return Enums.TrendType.InsufficientData;

        var midpoint = values.Count / 2;
        var first = values.Take(midpoint).Average();
        var second = values.Skip(midpoint).Average();
        var difference = second - first;

        return Math.Abs(difference) < GlucoseRules.TrendStabilityDeltaMgDl
            ? Enums.TrendType.Stable
            : difference > 0
                ? Enums.TrendType.Worsening
                : Enums.TrendType.Improving;
    }
}
