using Application.DTOs.DoctorPatient;

namespace Application.Interfaces;

public interface IDoctorPatientService
{
    Task<DoctorPatientRelationshipResponse> AddPatientAsync(
        Guid doctorUserId,
        CreateDoctorPatientRelationshipRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DoctorPatientRelationshipResponse>> GetDoctorPatientsAsync(
        Guid doctorUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientDoctorResponse>> GetPatientDoctorsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken);

    Task DeactivateRelationshipAsync(
        Guid doctorUserId,
        Guid relationshipId,
        CancellationToken cancellationToken);
}
