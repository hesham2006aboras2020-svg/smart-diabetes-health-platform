using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class DoctorPatientDetailsServiceTests
{
    [Fact]
    public async Task GetMedicationAdherenceAsync_ReturnsPatientScopedSummary()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var doctorUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "doctor@test.local",
            Email = "doctor@test.local",
            FullName = "Test Doctor",
            Role = Role.Doctor,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            User = doctorUser,
            LicenseNumber = "LIC-001",
            CreatedAt = DateTime.UtcNow
        };

        var patientUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "patient@test.local",
            Email = "patient@test.local",
            FullName = "Test Patient",
            Role = Role.Patient,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            User = patientUser,
            CreatedAt = DateTime.UtcNow
        };

        var relationship = new DoctorPatientRelationship
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            Status = RelationshipStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var medication = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Patient = patient,
            Name = "Test Medication",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var now = DateTime.UtcNow;
        medication.Logs.Add(new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Medication = medication,
            ScheduledTime = now.AddDays(-1),
            Status = MedicationLogStatus.Taken,
            TakenAt = now.AddDays(-1)
        });
        medication.Logs.Add(new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Medication = medication,
            ScheduledTime = now.AddDays(-2),
            Status = MedicationLogStatus.Missed
        });
        medication.Logs.Add(new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Medication = medication,
            ScheduledTime = now.AddDays(-3),
            Status = MedicationLogStatus.Skipped
        });

        db.Users.AddRange(doctorUser, patientUser);
        db.Doctors.Add(doctor);
        db.Patients.Add(patient);
        db.DoctorPatientRelationships.Add(relationship);
        db.Medications.Add(medication);
        await db.SaveChangesAsync();

        var service = new DoctorPatientDetailsService(db);

        var result = await service.GetMedicationAdherenceAsync(
            doctorUser.Id,
            patient.Id,
            7,
            CancellationToken.None);

        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal(3, result.TotalDosesLogged);
        Assert.Equal(1, result.TakenDoses);
        Assert.Equal(1, result.MissedDoses);
        Assert.Equal(1, result.SkippedDoses);
        Assert.Equal(33.33m, result.AdherencePercentage);
        Assert.Equal(1, result.ActiveMedicationCount);
    }

    [Fact]
    public async Task GetMedicationAdherenceAsync_RejectsUnlinkedPatient()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var service = new DoctorPatientDetailsService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetMedicationAdherenceAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                CancellationToken.None));
    }
}
