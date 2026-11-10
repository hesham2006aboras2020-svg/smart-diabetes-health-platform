using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DoctorPatientDetailsService(DiabetesDbContext db) : IDoctorPatientDetailsService
{
    private async Task EnsureLinkedAsync(Guid doctorUserId, Guid patientId, CancellationToken cancellationToken)
    {
        var allowed = await db.DoctorPatientRelationships.AnyAsync(
            x => x.PatientId == patientId &&
                 x.Doctor.UserId == doctorUserId &&
                 x.Doctor.User.AccountStatus == AccountStatus.Active &&
                 x.Status == RelationshipStatus.Active,
            cancellationToken);

        if (!allowed)
            throw new UnauthorizedAccessException("المريض غير مرتبط بهذا الطبيب.");
    }

    public async Task<DoctorPatientDetailsResponse> GetDetailsAsync(
        Guid doctorUserId, Guid patientId, CancellationToken cancellationToken)
    {
        await EnsureLinkedAsync(doctorUserId, patientId, cancellationToken);

        var patient = await db.Patients.AsNoTracking()
            .Where(x => x.Id == patientId)
            .Select(x => new
            {
                x.Id,
                Name = x.User.FullName,
                Email = x.User.Email!,
                x.DateOfBirth,
                x.Gender
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (patient is null)
            throw new KeyNotFoundException("المريض غير موجود.");

        var from = DateTime.UtcNow.AddDays(-7);
        var readings = await db.GlucoseReadings.AsNoTracking()
            .Where(x => x.PatientId == patientId && x.MeasurementDateTime >= from)
            .OrderBy(x => x.MeasurementDateTime)
            .Select(x => new { x.GlucoseValue, x.Unit, x.MeasurementDateTime })
            .ToListAsync(cancellationToken);

        var values = readings.Select(x => x.Unit == GlucoseUnit.MmolL ? x.GlucoseValue * 18m : x.GlucoseValue).ToList();

        decimal? average = values.Count == 0 ? null : Math.Round(values.Average(), 2);
        decimal? inRange = values.Count == 0 ? null : Math.Round(values.Count(x => x is >= 70m and <= 180m) * 100m / values.Count, 2);

        var trend = "InsufficientData";
        if (values.Count >= 4)
        {
            var midpoint = values.Count / 2;
            var difference = values.Skip(midpoint).Average() - values.Take(midpoint).Average();
            trend = Math.Abs(difference) < 10m ? "Stable" : difference > 0 ? "Rising" : "Falling";
        }

        var last = readings.LastOrDefault();
        var unreadAlerts = await db.Alerts.CountAsync(x => x.PatientId == patientId && !x.IsRead, cancellationToken);
        var activeMedications = await db.Medications.CountAsync(x => x.PatientId == patientId && x.IsActive, cancellationToken);

        return new DoctorPatientDetailsResponse(
            patient.Id,
            patient.Name,
            patient.Email,
            patient.DateOfBirth,
            patient.Gender,
            last is null ? null : last.MeasurementDateTime,
            last is null ? null : Math.Round(last.Unit == GlucoseUnit.MmolL ? last.GlucoseValue * 18m : last.GlucoseValue, 2),
            average,
            inRange,
            trend,
            unreadAlerts,
            activeMedications);
    }

    public async Task<IReadOnlyList<DoctorPatientReadingResponse>> GetReadingsAsync(
        Guid doctorUserId, Guid patientId, int days, CancellationToken cancellationToken)
    {
        if (days is < 1 or > 90)
            throw new ArgumentException("الفترة يجب أن تكون بين يوم واحد و90 يومًا.");

        await EnsureLinkedAsync(doctorUserId, patientId, cancellationToken);

        return await db.GlucoseReadings.AsNoTracking()
            .Where(x => x.PatientId == patientId &&
                        x.MeasurementDateTime >= DateTime.UtcNow.AddDays(-days))
            .OrderByDescending(x => x.MeasurementDateTime)
            .Take(500)
            .Select(x => new DoctorPatientReadingResponse(
                x.Id, x.GlucoseValue, x.Unit, x.MeasurementDateTime, x.MeasurementContext, x.Notes))
            .ToListAsync(cancellationToken);
    }
}