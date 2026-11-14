using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class HealthAnalyticsServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_CalculatesCoreMetricsAndNormalPriority()
    {
        await using var db = CreateDb();
        var patientUser = CreateUser();
        var patient = new Patient { Id = Guid.NewGuid(), UserId = patientUser.Id, User = patientUser, CreatedAt = DateTime.UtcNow };
        db.Users.Add(patientUser);
        db.Patients.Add(patient);

        for (var i = 0; i < 7; i++)
        {
            db.GlucoseReadings.Add(new GlucoseReading
            {
                Id = Guid.NewGuid(), PatientId = patient.Id, Patient = patient,
                GlucoseValue = 100 + i, Unit = GlucoseUnit.MgDl,
                MeasurementDateTime = DateTime.UtcNow.AddDays(-6 + i),
                MeasurementContext = MeasurementContext.Fasting,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var result = await new HealthAnalyticsService(db).AnalyzeAsync(patientUser.Id, 7, CancellationToken.None);

        Assert.Equal(7, result.ReadingCount);
        Assert.Equal(103m, result.AverageMgDl);
        Assert.Equal(100m, result.MinimumMgDl);
        Assert.Equal(106m, result.MaximumMgDl);
        Assert.Equal(100m, result.InRangePercentage);
        Assert.Equal(0m, result.HighPercentage);
        Assert.Equal(0m, result.LowPercentage);
        Assert.Equal("Stable", result.Trend);
        Assert.Equal("Normal", result.FollowUpPriority);
        Assert.Equal("Good", result.StabilityLevel);
        Assert.Equal(100m, result.StabilityScore);
    }

    [Fact]
    public async Task AnalyzeAsync_ConvertsMmolAndDetectsWorseningHighPriority()
    {
        await using var db = CreateDb();
        var patientUser = CreateUser();
        var patient = new Patient { Id = Guid.NewGuid(), UserId = patientUser.Id, User = patientUser, CreatedAt = DateTime.UtcNow };
        db.Users.Add(patientUser);
        db.Patients.Add(patient);

        for (var i = 0; i < 10; i++)
        {
            db.GlucoseReadings.Add(new GlucoseReading
            {
                Id = Guid.NewGuid(), PatientId = patient.Id, Patient = patient,
                GlucoseValue = i < 5 ? 8m : 15m, Unit = GlucoseUnit.MmolL,
                MeasurementDateTime = DateTime.UtcNow.AddDays(-9 + i),
                MeasurementContext = MeasurementContext.Fasting,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var result = await new HealthAnalyticsService(db).AnalyzeAsync(patientUser.Id, 10, CancellationToken.None);

        Assert.Equal(10, result.ReadingCount);
        Assert.Equal("Worsening", result.Trend);
        Assert.Equal("High", result.FollowUpPriority);
        Assert.Equal(50m, result.InRangePercentage);
        Assert.Equal(50m, result.HighPercentage);
        Assert.Equal(0m, result.LowPercentage);
    }

    [Fact]
    public async Task AnalyzeAsync_RejectsUnsupportedPeriod()
    {
        await using var db = CreateDb();
        var service = new HealthAnalyticsService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AnalyzeAsync(Guid.NewGuid(), 31, CancellationToken.None));
    }

    private static DiabetesDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<DiabetesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(), UserName = "analytics@test.local", Email = "analytics@test.local",
        FullName = "Analytics Test", Role = Role.Patient, AccountStatus = AccountStatus.Active,
        CreatedAt = DateTime.UtcNow
    };
}
