using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class MedicationServiceTests
{
    [Fact]
    public async Task GetAdherenceSummaryAsync_CalculatesTakenMissedSkippedAndPercentage()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var user = new User
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
            UserId = user.Id,
            User = user,
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
            Status = MedicationLogStatus.Taken,
            TakenAt = now.AddDays(-2)
        });
        medication.Logs.Add(new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Medication = medication,
            ScheduledTime = now.AddDays(-3),
            Status = MedicationLogStatus.Missed
        });
        medication.Logs.Add(new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Medication = medication,
            ScheduledTime = now.AddDays(-4),
            Status = MedicationLogStatus.Skipped
        });

        db.Users.Add(user);
        db.Patients.Add(patient);
        db.Medications.Add(medication);
        await db.SaveChangesAsync();

        var service = new MedicationService(db);

        var result = await service.GetAdherenceSummaryAsync(
            user.Id,
            7,
            CancellationToken.None);

        Assert.Equal(4, result.TotalDosesLogged);
        Assert.Equal(2, result.TakenDoses);
        Assert.Equal(1, result.MissedDoses);
        Assert.Equal(1, result.SkippedDoses);
        Assert.Equal(50m, result.AdherencePercentage);
        Assert.Equal(1, result.ActiveMedicationCount);
    }

    [Fact]
    public async Task GetAdherenceSummaryAsync_RejectsInvalidDays()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var service = new MedicationService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAdherenceSummaryAsync(Guid.NewGuid(), 0, CancellationToken.None));
    }
}
