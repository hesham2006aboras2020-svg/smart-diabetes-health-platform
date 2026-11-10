using Application.DTOs.Glucose;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Rules;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class GlucoseService(DiabetesDbContext dbContext) : IGlucoseService
{
    public async Task<GlucoseReadingResponse> AddReadingAsync(
        Guid userId,
        CreateGlucoseReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var patient = await dbContext.Patients
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (patient is null)
            throw new UnauthorizedAccessException("حساب المريض غير موجود.");

        var valueMgDl = GlucoseValueConverter.ToMgDl(request.GlucoseValue, request.Unit);
        var status = GetStatus(valueMgDl);

        var reading = new GlucoseReading
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            GlucoseValue = request.GlucoseValue,
            Unit = request.Unit,
            MeasurementDateTime = request.MeasurementDateTime,
            MeasurementContext = request.MeasurementContext,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.GlucoseReadings.Add(reading);

        if (status != "Normal")
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                GlucoseReadingId = reading.Id,
                AlertType = status == "Low" ? AlertType.LowGlucose : AlertType.HighGlucose,
                Severity = AlertSeverity.Medium,
                Message = status == "Low"
                    ? "تم تسجيل قراءة سكر منخفضة وفق قواعد المنصة."
                    : "تم تسجيل قراءة سكر مرتفعة وفق قواعد المنصة.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Alerts.Add(alert);
            dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AlertId = alert.Id,
                Title = status == "Low" ? "تنبيه قراءة منخفضة" : "تنبيه قراءة مرتفعة",
                Body = alert.Message,
                Type = NotificationType.Alert,
                IsSent = false,
                SentAt = null,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(reading, status);
    }

    public async Task<IReadOnlyList<GlucoseReadingResponse>> GetReadingsAsync(
        Guid userId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("تاريخ البداية يجب أن يسبق تاريخ النهاية.");

        var query = dbContext.GlucoseReadings
            .AsNoTracking()
            .Where(x => x.Patient.UserId == userId);

        if (from.HasValue)
            query = query.Where(x => x.MeasurementDateTime >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.MeasurementDateTime <= to.Value);

        var readings = await query
            .OrderByDescending(x => x.MeasurementDateTime)
            .Take(500)
            .ToListAsync(cancellationToken);

        return readings
            .Select(x => Map(x, GetStatus(GlucoseValueConverter.ToMgDl(x.GlucoseValue, x.Unit))))
            .ToList();
    }

    public async Task<GlucoseSummaryResponse> GetSummaryAsync(
        Guid userId,
        int days,
        CancellationToken cancellationToken = default)
    {
        if (days is < 1 or > 90)
            throw new ArgumentException("الفترة يجب أن تكون بين يوم واحد و90 يومًا.");

        var from = DateTime.UtcNow.AddDays(-days);

        var readings = await dbContext.GlucoseReadings
            .AsNoTracking()
            .Where(x => x.Patient.UserId == userId && x.MeasurementDateTime >= from)
            .OrderBy(x => x.MeasurementDateTime)
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
            return new GlucoseSummaryResponse(0, null, null, null, null, null, null, "InsufficientData");

        var values = readings.Select(x => GlucoseValueConverter.ToMgDl(x.GlucoseValue, x.Unit)).ToList();
        var average = values.Average();
        var inRange = values.Count(x => x is >= GlucoseRules.LowThresholdMgDl and <= GlucoseRules.HighThresholdMgDl);
        var high = values.Count(x => x > GlucoseRules.HighThresholdMgDl);
        var low = values.Count(x => x < GlucoseRules.LowThresholdMgDl);

        return new GlucoseSummaryResponse(
            values.Count,
            Math.Round(average, 2),
            Math.Round(values.Min(), 2),
            Math.Round(values.Max(), 2),
            Math.Round(inRange * 100m / values.Count, 2),
            Math.Round(high * 100m / values.Count, 2),
            Math.Round(low * 100m / values.Count, 2),
            CalculateTrend(values));
    }

    private static void ValidateRequest(CreateGlucoseReadingRequest request)
    {
        if (request.GlucoseValue <= 0)
            throw new ArgumentException("قيمة سكر الدم يجب أن تكون أكبر من صفر.");

        var valueMgDl = GlucoseValueConverter.ToMgDl(request.GlucoseValue, request.Unit);

        if (valueMgDl > 1000)
            throw new ArgumentException("قيمة سكر الدم المدخلة خارج النطاق المدعوم في المنصة.");

        if (request.MeasurementDateTime > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("وقت القياس لا يمكن أن يكون في المستقبل.");

        if (request.Notes?.Length > 500)
            throw new ArgumentException("الملاحظات يجب ألا تتجاوز 500 حرف.");
    }

    private static string GetStatus(decimal valueMgDl) =>
        valueMgDl < GlucoseRules.LowThresholdMgDl ? "Low" :
        valueMgDl > GlucoseRules.HighThresholdMgDl ? "High" :
        "Normal";

    private static string CalculateTrend(IReadOnlyList<decimal> values)
    {
        if (values.Count < 4)
            return "InsufficientData";

        var midpoint = values.Count / 2;
        var firstAverage = values.Take(midpoint).Average();
        var secondAverage = values.Skip(midpoint).Average();
        var difference = secondAverage - firstAverage;

        if (Math.Abs(difference) < GlucoseRules.TrendStabilityDeltaMgDl)
            return "Stable";

        return difference > 0 ? TrendType.Worsening.ToString() : TrendType.Improving.ToString();
    }

    private static GlucoseReadingResponse Map(GlucoseReading reading, string status) =>
        new(
            reading.Id,
            reading.GlucoseValue,
            reading.Unit,
            reading.MeasurementDateTime,
            reading.MeasurementContext,
            reading.Notes,
            reading.CreatedAt,
            status);
}
