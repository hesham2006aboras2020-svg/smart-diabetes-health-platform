using Application.DTOs.DoctorDashboard;

namespace Application.Interfaces;

public interface IDoctorPatientDetailsService
{
    Task<DoctorPatientDetailsResponse> GetDetailsAsync(Guid doctorUserId, Guid patientId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DoctorPatientReadingResponse>> GetReadingsAsync(Guid doctorUserId, Guid patientId, int days, CancellationToken cancellationToken);

    Task<DoctorPatientMedicationAdherenceResponse> GetMedicationAdherenceAsync(
        Guid doctorUserId, Guid patientId, int days, CancellationToken cancellationToken);
}