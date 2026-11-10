using Application.DTOs.Alerts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AlertService(DiabetesDbContext db) : IAlertService
{
    public async Task<IReadOnlyList<AlertResponse>> GetMyAlertsAsync(
        Guid patientUserId, bool unreadOnly, CancellationToken cancellationToken)
    {
        var query = db.Alerts.AsNoTracking()
            .Where(x => x.Patient.UserId == patientUserId);

        if (unreadOnly)
            query = query.Where(x => !x.IsRead);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new AlertResponse(
                x.Id, x.PatientId, x.GlucoseReadingId,
                x.AlertType.ToString(), x.Severity.ToString(),
                x.Message, x.IsRead, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid patientUserId, Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await db.Alerts
            .FirstOrDefaultAsync(x => x.Id == alertId && x.Patient.UserId == patientUserId, cancellationToken);

        if (alert is null)
            throw new KeyNotFoundException("التنبيه غير موجود.");

        alert.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DetectRepeatedPatternsAsync(
        Guid patientUserId, int days, CancellationToken cancellationToken)
    {
        if (days is < 7 or > 30)
            throw new ArgumentException("الفترة يجب أن تكون بين 7 و30 يومًا.");

        var from = DateTime.UtcNow.AddDays(-days);

        var readings = await db.GlucoseReadings.AsNoTracking()
            .Where(x => x.Patient.UserId == patientUserId && x.MeasurementDateTime >= from)
            .OrderBy(x => x.MeasurementDateTime)
            .Select(x => new { x.Id, x.GlucoseValue, x.Unit, x.MeasurementDateTime })
            .ToListAsync(cancellationToken);

        var values = readings.Select(x => new
        {
            x.Id,
            Value = x.Unit == GlucoseUnit.MmolL ? x.GlucoseValue * 18m : x.GlucoseValue,
            x.MeasurementDateTime
        }).ToList();

        var high = values.Count(x => x.Value > 180m);
        var low = values.Count(x => x.Value < 70m);

        var created = 0;

        if (high >= 3)
            created += await CreatePatternAlertIfNeededAsync(
                patientUserId, AlertType.RepeatedAbnormalReadings, AlertSeverity.Medium,
                "تم رصد تكرار لقراءات السكر المرتفعة وفق قواعد المنصة.", from, cancellationToken);

        if (low >= 3)
            created += await CreatePatternAlertIfNeededAsync(
                patientUserId, AlertType.RepeatedAbnormalReadings, AlertSeverity.Medium,
                "تم رصد تكرار لقراءات السكر المنخفضة وفق قواعد المنصة.", from, cancellationToken);

        return created;
    }

    private async Task<int> CreatePatternAlertIfNeededAsync(
        Guid patientUserId, AlertType type, AlertSeverity severity, string message,
        DateTime from, CancellationToken cancellationToken)
    {
        var exists = await db.Alerts.AnyAsync(
            x => x.Patient.UserId == patientUserId &&
                 x.AlertType == type &&
                 x.CreatedAt >= from,
            cancellationToken);

        if (exists)
            return 0;

        var patientId = await db.Patients
            .Where(x => x.UserId == patientUserId)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        db.Alerts.Add(new Alert
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            AlertType = type,
            Severity = severity,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return 1;
    }
}
