using Application.DTOs.HealthReport;
using Application.Interfaces;
using Domain.Enums;
using Domain.Rules;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class HealthReportService(DiabetesDbContext db) : IHealthReportService
{
    public async Task<HealthReportResponse> CreateAsync(Guid patientUserId, CreateHealthReportRequest request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
            throw new ArgumentException("تاريخ البداية يجب ألا يتجاوز تاريخ النهاية.");

        if (request.EndDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("لا يمكن إنشاء تقرير لفترة مستقبلية.");

        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == patientUserId, cancellationToken);
        if (patient is null) throw new UnauthorizedAccessException("حساب المريض غير موجود.");

        var start = request.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusive = request.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var readings = await db.GlucoseReadings.AsNoTracking()
            .Where(x => x.PatientId == patient.Id && x.MeasurementDateTime >= start && x.MeasurementDateTime < endExclusive)
            .OrderBy(x => x.MeasurementDateTime)
            .Select(x => new { x.GlucoseValue, x.Unit, x.MeasurementDateTime })
            .ToListAsync(cancellationToken);

        var values = readings.Select(x => GlucoseValueConverter.ToMgDl(x.GlucoseValue, x.Unit)).ToList();
        var stabilityReadings = readings
            .Select((x, index) => new StabilityReading(values[index], x.MeasurementDateTime))
            .ToList();

        var report = new Domain.Entities.HealthReport
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NumberOfReadings = values.Count,
            AverageGlucose = values.Count == 0 ? null : Math.Round(values.Average(), 2),
            MinimumGlucose = values.Count == 0 ? null : values.Min(),
            MaximumGlucose = values.Count == 0 ? null : values.Max(),
            InRangePercentage = values.Count == 0 ? null : Math.Round(values.Count(v => v is >= GlucoseRules.LowThresholdMgDl and <= GlucoseRules.HighThresholdMgDl) * 100m / values.Count, 2),
            HighPercentage = values.Count == 0 ? null : Math.Round(values.Count(v => v > GlucoseRules.HighThresholdMgDl) * 100m / values.Count, 2),
            LowPercentage = values.Count == 0 ? null : Math.Round(values.Count(v => v < GlucoseRules.LowThresholdMgDl) * 100m / values.Count, 2),
            Trend = CalculateTrend(values),
            StabilityScore = StabilityScoreCalculator.Calculate(stabilityReadings, request.EndDate.DayNumber - request.StartDate.DayNumber + 1),
            CreatedAt = DateTime.UtcNow
        };

        db.HealthReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);
        return Map(report);
    }

    public async Task<IReadOnlyList<HealthReportResponse>> GetAllAsync(Guid patientUserId, CancellationToken cancellationToken)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == patientUserId, cancellationToken);
        if (patient is null) throw new UnauthorizedAccessException("حساب المريض غير موجود.");

        return await db.HealthReports.AsNoTracking()
            .Where(x => x.PatientId == patient.Id)
            .OrderByDescending(x => x.EndDate).ThenByDescending(x => x.CreatedAt)
            .Select(x => new HealthReportResponse(
                x.Id, x.PatientId, x.StartDate, x.EndDate, x.NumberOfReadings,
                x.AverageGlucose, x.MinimumGlucose, x.MaximumGlucose,
                x.InRangePercentage, x.HighPercentage, x.LowPercentage,
                x.Trend.ToString(), x.StabilityScore, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<HealthReportResponse?> GetLatestAsync(Guid patientUserId, CancellationToken cancellationToken)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == patientUserId, cancellationToken);
        if (patient is null) throw new UnauthorizedAccessException("حساب المريض غير موجود.");

        return await db.HealthReports.AsNoTracking()
            .Where(x => x.PatientId == patient.Id)
            .OrderByDescending(x => x.EndDate).ThenByDescending(x => x.CreatedAt)
            .Select(x => new HealthReportResponse(
                x.Id, x.PatientId, x.StartDate, x.EndDate, x.NumberOfReadings,
                x.AverageGlucose, x.MinimumGlucose, x.MaximumGlucose,
                x.InRangePercentage, x.HighPercentage, x.LowPercentage,
                x.Trend.ToString(), x.StabilityScore, x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static TrendType CalculateTrend(IReadOnlyList<decimal> values)
    {
        if (values.Count < 4) return TrendType.InsufficientData;
        var midpoint = values.Count / 2;
        var first = values.Take(midpoint).Average();
        var second = values.Skip(midpoint).Average();
        var difference = second - first;
        return Math.Abs(difference) < GlucoseRules.TrendStabilityDeltaMgDl ? TrendType.Stable : difference > 0 ? TrendType.Worsening : TrendType.Improving;
    }

    private static HealthReportResponse Map(Domain.Entities.HealthReport report) =>
        new(report.Id, report.PatientId, report.StartDate, report.EndDate, report.NumberOfReadings,
            report.AverageGlucose, report.MinimumGlucose, report.MaximumGlucose,
            report.InRangePercentage, report.HighPercentage, report.LowPercentage,
            report.Trend.ToString(), report.StabilityScore, report.CreatedAt);
}
