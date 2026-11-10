using Application.DTOs.DoctorDashboard;

namespace Application.Interfaces;

public interface IDoctorPatientAlertsService
{
    Task<IReadOnlyList<DoctorPatientAlertResponse>> GetAlertsAsync(
        Guid doctorUserId, Guid patientId, bool unreadOnly, CancellationToken cancellationToken);
}