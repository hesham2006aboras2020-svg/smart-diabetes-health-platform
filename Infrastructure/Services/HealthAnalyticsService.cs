using Application.DTOs.Health;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class HealthAnalyticsService(DiabetesDbContext db) : IHealthAnalyticsService
{
    private const decimal LowThresholdMgDl = 70m;
    private const decimal HighThresholdMgDl = 180m;

    public async Task<HealthAnalyticsResponse> AnalyzeAsync(Guid patientUserId, int days, CancellationToken cancellationToken)
    {
        if (days is < 7 or > 30)
            throw new ArgumentException("الفترة يجب أن تكون بين 7 و30 يومًا.");

        var from = DateTime.UtcNow.AddDays(-days);

        var readings = await db.GlucoseReadings.AsNoTracking()
            .Where(x => x.Patient.UserId == patientUserId &&
                        x.MeasurementDateTime >= from &&
                        x.MeasurementDateTime <= DateTime.UtcNow)
            .OrderBy(x => x.MeasurementDateTime)
            .Select(x => new { x.GlucoseValue, x.Unit })
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
            return new HealthAnalyticsResponse(days, 0, null, null, null, null, null, null,
                "InsufficientData", null, "InsufficientData", "Normal");

        var values = readings.Select(x => ToMgDl(x.GlucoseValue, x.Unit)).ToList();
        var inRangePercentage = values.Count(x => x is >= LowThresholdMgDl and <= HighThresholdMgDl) * 100m / values.Count;
        var highPercentage = values.Count(x => x > HighThresholdMgDl) * 100m / values.Count;
        var lowPercentage = values.Count(x => x < LowThresholdMgDl) * 100m / values.Count;
        var trend = CalculateTrend(values);

        var abnormalPenalty = Math.Min(40m, (highPercentage + lowPercentage) * 0.8m);
        var trendPenalty = trend is "Rising" ? 10m : trend is "Falling" ? 5m : 0m;
        var coverageBonus = values.Count >= 7 ? 10m : values.Count / 7m * 10m;
        var score = Math.Clamp(inRangePercentage - abnormalPenalty + coverageBonus - trendPenalty, 0m, 100m);

        var stabilityLevel = score >= 80m ? "Good" : score >= 60m ? "Moderate" : "NeedsFollowUp";
        var priority = CalculateFollowUpPriority(lowPercentage, highPercentage, trend, values.Count);

        return new HealthAnalyticsResponse(
            days, values.Count,
            Math.Round(values.Average(), 2),
            Math.Round(values.Min(), 2),
            Math.Round(values.Max(), 2),
            Math.Round(inRangePercentage, 2),
            Math.Round(highPercentage, 2),
            Math.Round(lowPercentage, 2),
            trend,
            Math.Round(score, 2),
            stabilityLevel,
            priority);
    }

    private static decimal ToMgDl(decimal value, GlucoseUnit unit) => unit switch
    {
        GlucoseUnit.MgDl => value,
        GlucoseUnit.MmolL => value * 18m,
        _ => throw new ArgumentException("وحدة قياس السكر غير مدعومة.")
    };

    private static string CalculateTrend(IReadOnlyList<decimal> values)
    {
        if (values.Count < 4) return "InsufficientData";
        var midpoint = values.Count / 2;
        var difference = values.Skip(midpoint).Average() - values.Take(midpoint).Average();
        if (Math.Abs(difference) < 10m) return "Stable";
        return difference > 0 ? "Rising" : "Falling";
    }

    private static string CalculateFollowUpPriority(decimal lowPercentage, decimal highPercentage, string trend, int count)
    {
        if (count < 7) return "LowData";
        if (lowPercentage >= 15m || highPercentage >= 30m) return "High";
        if (lowPercentage >= 10m || highPercentage >= 20m || trend == "Rising") return "Medium";
        return "Normal";
    }
}
