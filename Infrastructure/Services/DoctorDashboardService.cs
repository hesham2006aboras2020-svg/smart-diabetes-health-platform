using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Domain.Enums;
using Domain.Rules;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DoctorDashboardService(DiabetesDbContext db) : IDoctorDashboardService
{
    public async Task<IReadOnlyList<DoctorPatientDashboardResponse>> GetPatientsAsync(
        Guid doctorUserId, CancellationToken cancellationToken)
    {
        var doctor = await db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == doctorUserId && x.User.AccountStatus == AccountStatus.Active, cancellationToken);

        if (doctor is null)
            throw new UnauthorizedAccessException("حساب الطبيب غير نشط أو غير موجود.");

        var patients = await db.DoctorPatientRelationships
            .AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id && x.Status == RelationshipStatus.Active)
            .Select(x => new
            {
                x.PatientId,
                PatientName = x.Patient.User.FullName
            })
            .ToListAsync(cancellationToken);

        var result = new List<DoctorPatientDashboardResponse>();

        foreach (var patient in patients)
        {
            var readings = await db.GlucoseReadings
                .AsNoTracking()
                .Where(x => x.PatientId == patient.PatientId &&
                            x.MeasurementDateTime >= DateTime.UtcNow.AddDays(-7))
                .OrderBy(x => x.MeasurementDateTime)
                .Select(x => new { x.GlucoseValue, x.Unit, x.MeasurementDateTime })
                .ToListAsync(cancellationToken);

            var alerts = await db.Alerts
                .AsNoTracking()
                .CountAsync(x => x.PatientId == patient.PatientId && !x.IsRead, cancellationToken);

            if (readings.Count == 0)
            {
                result.Add(new DoctorPatientDashboardResponse(
                    patient.PatientId, patient.PatientName, null, null, null, null,
                    "InsufficientData", null, "InsufficientData", "LowData", alerts);
                continue;
            }

            var values = readings.Select(x => GlucoseValueConverter.ToMgDl(x.GlucoseValue, x.Unit)).ToList();
            var stabilityReadings = readings
                .Select((x, index) => new StabilityReading(values[index], x.MeasurementDateTime))
                .ToList();

            var inRange = values.Count(x => x is >= GlucoseRules.LowThresholdMgDl and <= GlucoseRules.HighThresholdMgDl) * 100m / values.Count;
            var high = values.Count(x => x > GlucoseRules.HighThresholdMgDl) * 100m / values.Count;
            var low = values.Count(x => x < GlucoseRules.LowThresholdMgDl) * 100m / values.Count;

            var midpoint = values.Count / 2;
            var trend = values.Count < 4
                ? "InsufficientData"
                : Math.Abs(values.Skip(midpoint).Average() - values.Take(midpoint).Average()) < GlucoseRules.TrendStabilityDeltaMgDl
                    ? TrendType.Stable.ToString()
                    : values.Skip(midpoint).Average() > values.Take(midpoint).Average() ? TrendType.Worsening.ToString() : TrendType.Improving.ToString();

            var score = StabilityScoreCalculator.Calculate(stabilityReadings, 7);
            var level = score is null
                ? "InsufficientData"
                : score >= 80m ? "Good" : score >= 60m ? "Moderate" : "NeedsFollowUp";

            var priority = score is null
                ? "LowData"
                : low >= 15m || high >= 30m
                    ? "High"
                    : low >= 10m || high >= 20m || trend == TrendType.Worsening.ToString()
                        ? "Medium"
                        : "Normal";

            var last = readings[^1];

            result.Add(new DoctorPatientDashboardResponse(
                patient.PatientId,
                patient.PatientName,
                last.MeasurementDateTime,
                Math.Round(GlucoseValueConverter.ToMgDl(last.GlucoseValue, last.Unit), 2),
                Math.Round(values.Average(), 2),
                Math.Round(inRange, 2),
                trend,
                score,
                level,
                priority,
                alerts));
        }

        return result
            .OrderByDescending(x => x.FollowUpPriority == "High")
            .ThenByDescending(x => x.FollowUpPriority == "Medium")
            .ThenByDescending(x => x.UnreadAlertCount)
            .ThenBy(x => x.PatientName)
            .ToList();
    }
}
