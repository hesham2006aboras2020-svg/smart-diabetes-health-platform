using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DoctorPatientAlertsService(DiabetesDbContext db) : IDoctorPatientAlertsService
{
    public async Task<IReadOnlyList<DoctorPatientAlertResponse>> GetAlertsAsync(
        Guid doctorUserId, Guid patientId, bool unreadOnly, CancellationToken cancellationToken)
    {
        var linked = await db.DoctorPatientRelationships.AnyAsync(
            x => x.PatientId == patientId &&
                 x.Doctor.UserId == doctorUserId &&
                 x.Doctor.User.AccountStatus == AccountStatus.Active &&
                 x.Status == RelationshipStatus.Active,
            cancellationToken);

        if (!linked)
            throw new UnauthorizedAccessException("المريض غير مرتبط بهذا الطبيب.");

        var query = db.Alerts.AsNoTracking()
            .Where(x => x.PatientId == patientId);

        if (unreadOnly)
            query = query.Where(x => !x.IsRead);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new DoctorPatientAlertResponse(
                x.Id, x.AlertType, x.Severity, x.AlertType.ToString(), x.Message, x.CreatedAt, x.IsRead))
            .ToListAsync(cancellationToken);
    }
}