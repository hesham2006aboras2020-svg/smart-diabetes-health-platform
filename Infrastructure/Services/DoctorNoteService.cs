using Application.DTOs.DoctorNotes;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DoctorNoteService(DiabetesDbContext db) : IDoctorNoteService
{
    public async Task<DoctorNoteResponse> CreateAsync(
        Guid doctorUserId,
        CreateDoctorNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient is required.");

        if (string.IsNullOrWhiteSpace(request.Note) || request.Note.Trim().Length > 2000)
            throw new ArgumentException("Note is required and must not exceed 2000 characters.");

        var doctor = await db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == doctorUserId && x.User.AccountStatus == AccountStatus.Active, cancellationToken);

        if (doctor is null)
            throw new UnauthorizedAccessException("Active doctor account is required.");

        var patientExists = await db.Patients.AnyAsync(x => x.Id == request.PatientId, cancellationToken);
        if (!patientExists)
            throw new KeyNotFoundException("Patient was not found.");

        var linked = await db.DoctorPatientRelationships.AnyAsync(
            x => x.DoctorId == doctor.Id &&
                 x.PatientId == request.PatientId &&
                 x.Status == RelationshipStatus.Active,
            cancellationToken);

        if (!linked)
            throw new UnauthorizedAccessException("Doctor is not linked to this patient.");

        var note = new DoctorNote
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            PatientId = request.PatientId,
            Note = request.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.DoctorNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        var doctorName = await db.Users
            .Where(x => x.Id == doctor.UserId)
            .Select(x => x.FullName)
            .SingleAsync(cancellationToken);

        return new DoctorNoteResponse(note.Id, note.DoctorId, note.PatientId, doctorName, note.Note, note.CreatedAt, note.UpdatedAt);
    }

    public async Task<IReadOnlyList<DoctorNoteResponse>> GetForPatientAsync(
        Guid doctorUserId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var doctor = await db.Doctors.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == doctorUserId, cancellationToken);
        if (doctor is null)
            throw new UnauthorizedAccessException("Doctor account was not found.");

        var linked = await db.DoctorPatientRelationships.AnyAsync(
            x => x.DoctorId == doctor.Id && x.PatientId == patientId && x.Status == RelationshipStatus.Active,
            cancellationToken);

        if (!linked)
            throw new UnauthorizedAccessException("Doctor is not linked to this patient.");

        return await db.DoctorNotes.AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id && x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DoctorNoteResponse(
                x.Id, x.DoctorId, x.PatientId, x.Doctor.User.FullName,
                x.Note, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorNoteResponse>> GetMyNotesAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == patientUserId, cancellationToken);
        if (patient is null)
            throw new UnauthorizedAccessException("Patient account was not found.");

        return await db.DoctorNotes.AsNoTracking()
            .Where(x => x.PatientId == patient.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DoctorNoteResponse(
                x.Id, x.DoctorId, x.PatientId, x.Doctor.User.FullName,
                x.Note, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
