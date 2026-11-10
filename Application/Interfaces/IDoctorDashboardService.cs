using Application.DTOs.DoctorDashboard;

namespace Application.Interfaces;

public interface IDoctorDashboardService
{
    Task<IReadOnlyList<DoctorPatientDashboardResponse>> GetPatientsAsync(
        Guid doctorUserId, CancellationToken cancellationToken);
}
