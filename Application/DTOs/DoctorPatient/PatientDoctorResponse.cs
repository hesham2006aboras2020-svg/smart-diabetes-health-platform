namespace Application.DTOs.DoctorPatient;

public sealed record PatientDoctorResponse(
    Guid RelationshipId,
    Guid DoctorId,
    string DoctorName,
    string DoctorEmail,
    string LicenseNumber,
    string? Specialization,
    string? ClinicName,
    string Status,
    DateTime CreatedAt);
