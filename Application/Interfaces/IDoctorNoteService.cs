using Application.DTOs.DoctorNotes;

namespace Application.Interfaces;

public interface IDoctorNoteService
{
    Task<DoctorNoteResponse> CreateAsync(Guid doctorUserId, CreateDoctorNoteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<DoctorNoteResponse>> GetForPatientAsync(Guid doctorUserId, Guid patientId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DoctorNoteResponse>> GetMyNotesAsync(Guid patientUserId, CancellationToken cancellationToken);
}
