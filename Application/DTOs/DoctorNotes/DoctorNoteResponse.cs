namespace Application.DTOs.DoctorNotes;

public sealed record DoctorNoteResponse(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    string DoctorName,
    string Note,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
