using Domain.Enums;

namespace Domain.Rules;

public readonly record struct StabilityReading(decimal ValueMgDl, DateTime MeasurementDateTime);

public static class StabilityScoreCalculator
{
    public const int MinimumReadings = 7;

    public static decimal? Calculate(IReadOnlyList<StabilityReading> readings, int windowDays)
    {
        if (!HasSufficientData(readings, windowDays))
            return null;

        var inRange = readings.Count(x => x.ValueMgDl is >= GlucoseRules.LowThresholdMgDl and <= GlucoseRules.HighThresholdMgDl) * 100m / readings.Count;
        var high = readings.Count(x => x.ValueMgDl > GlucoseRules.HighThresholdMgDl) * 100m / readings.Count;
        var low = readings.Count(x => x.ValueMgDl < GlucoseRules.LowThresholdMgDl) * 100m / readings.Count;

        var trend = CalculateTrend(readings.Select(x => x.ValueMgDl).ToList());
        var penalty = Math.Min(40m, (high + low) * 0.8m);
        var trendPenalty = trend == TrendType.Worsening ? 10m : trend == TrendType.Improving ? 5m : 0m;
        const decimal dataBonus = 10m;

        return Math.Round(Math.Clamp(inRange - penalty + dataBonus - trendPenalty, 0m, 100m), 2);
    }

    public static bool HasSufficientData(IReadOnlyList<StabilityReading> readings, int windowDays) =>
        readings.Count >= MinimumReadings &&
        readings.Select(x => x.MeasurementDateTime.Date).Distinct().Count() >= RequiredDistinctDays(windowDays);

    public static int RequiredDistinctDays(int windowDays) =>
        Math.Max(2, (int)Math.Ceiling(windowDays * 0.25m));

    private static TrendType CalculateTrend(IReadOnlyList<decimal> values)
    {
        if (values.Count < 4) return TrendType.InsufficientData;

        var midpoint = values.Count / 2;
        var first = values.Take(midpoint).Average();
        var second = values.Skip(midpoint).Average();
        var difference = second - first;

        return Math.Abs(difference) < GlucoseRules.TrendStabilityDeltaMgDl
            ? TrendType.Stable
            : difference > 0
                ? TrendType.Worsening
                : TrendType.Improving;
    }
}
