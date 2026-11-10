using Application.DTOs.Patient;

namespace Application.Interfaces;

public interface IPatientProfileService
{
    Task<PatientProfileResponse> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<PatientProfileResponse> UpdateAsync(
        Guid userId,
        UpdatePatientProfileRequest request,
        CancellationToken cancellationToken);
}