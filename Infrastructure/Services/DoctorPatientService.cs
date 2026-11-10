using Application.DTOs.DoctorPatient;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class DoctorPatientService(DiabetesDbContext db) : IDoctorPatientService
{
    public async Task<DoctorPatientRelationshipResponse> AddPatientAsync(
        Guid doctorUserId,
        CreateDoctorPatientRelationshipRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PatientEmail))
            throw new ArgumentException("Patient email is required.");

        var doctor = await db.Doctors
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == doctorUserId, cancellationToken);

        if (doctor is null || doctor.User.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("Active doctor account is required.");

        var email = request.PatientEmail.Trim().ToLowerInvariant();
        var patient = await db.Patients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.Email == email, cancellationToken);

        if (patient is null)
            throw new KeyNotFoundException("Patient was not found.");

        var existing = await db.DoctorPatientRelationships
            .FirstOrDefaultAsync(x => x.DoctorId == doctor.Id && x.PatientId == patient.Id, cancellationToken);

        if (existing is not null)
        {
            if (existing.Status == RelationshipStatus.Active)
                throw new InvalidOperationException("This patient is already linked to the doctor.");

            existing.Status = RelationshipStatus.Active;
            existing.EndedAt = null;
        }
        else
        {
            existing = new DoctorPatientRelationship
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                Status = RelationshipStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            db.DoctorPatientRelationships.Add(existing);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new DoctorPatientRelationshipResponse(
            existing.Id,
            doctor.Id,
            patient.Id,
            patient.User.FullName,
            patient.User.Email!,
            existing.Status.ToString(),
            existing.CreatedAt,
            existing.EndedAt);
    }

    public async Task<IReadOnlyList<DoctorPatientRelationshipResponse>> GetDoctorPatientsAsync(
        Guid doctorUserId,
        CancellationToken cancellationToken)
    {
        var doctor = await db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == doctorUserId, cancellationToken);

        if (doctor is null)
            throw new UnauthorizedAccessException("Doctor account was not found.");

        return await db.DoctorPatientRelationships
            .AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id && x.Status == RelationshipStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DoctorPatientRelationshipResponse(
                x.Id,
                x.DoctorId,
                x.PatientId,
                x.Patient.User.FullName,
                x.Patient.User.Email!,
                x.Status.ToString(),
                x.CreatedAt,
                x.EndedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientDoctorResponse>> GetPatientDoctorsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == patientUserId, cancellationToken);

        if (patient is null)
            throw new UnauthorizedAccessException("Patient account was not found.");

        return await db.DoctorPatientRelationships
            .AsNoTracking()
            .Where(x => x.PatientId == patient.Id && x.Status == RelationshipStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PatientDoctorResponse(
                x.Id,
                x.DoctorId,
                x.Doctor.User.FullName,
                x.Doctor.User.Email!,
                x.Doctor.LicenseNumber,
                x.Doctor.Specialization,
                x.Doctor.ClinicName,
                x.Status.ToString(),
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task DeactivateRelationshipAsync(
        Guid doctorUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var doctor = await db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == doctorUserId, cancellationToken);

        if (doctor is null)
            throw new UnauthorizedAccessException("Doctor account was not found.");

        var relationship = await db.DoctorPatientRelationships
            .FirstOrDefaultAsync(x => x.Id == relationshipId && x.DoctorId == doctor.Id, cancellationToken);

        if (relationship is null)
            throw new KeyNotFoundException("Doctor-patient relationship was not found.");

        if (relationship.Status == RelationshipStatus.Inactive)
            return;

        relationship.Status = RelationshipStatus.Inactive;
        relationship.EndedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
