namespace Application.DTOs.DoctorPatient;

public sealed record DoctorPatientRelationshipResponse(
    Guid RelationshipId,
    Guid DoctorId,
    Guid PatientId,
    string PatientName,
    string PatientEmail,
    string Status,
    DateTime CreatedAt,
    DateTime? EndedAt);
