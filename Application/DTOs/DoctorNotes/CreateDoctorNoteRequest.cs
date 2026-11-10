namespace Application.DTOs.DoctorNotes;

public sealed record CreateDoctorNoteRequest(Guid PatientId, string Note);
