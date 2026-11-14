using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class AdminDashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsRequiredCountsAndThirtyDayAuditWindow()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var activePatientUser = CreateUser("active-patient@test.local", Role.Patient, AccountStatus.Active);
        var inactivePatientUser = CreateUser("inactive-patient@test.local", Role.Patient, AccountStatus.Suspended);
        var activeDoctorUser = CreateUser("active-doctor@test.local", Role.Doctor, AccountStatus.Active);
        var pendingDoctorUser = CreateUser("pending-doctor@test.local", Role.Doctor, AccountStatus.Pending);

        var activePatient = new Patient { Id = Guid.NewGuid(), UserId = activePatientUser.Id, User = activePatientUser, CreatedAt = DateTime.UtcNow };
        var inactivePatient = new Patient { Id = Guid.NewGuid(), UserId = inactivePatientUser.Id, User = inactivePatientUser, CreatedAt = DateTime.UtcNow };
        var activeDoctor = new Doctor { Id = Guid.NewGuid(), UserId = activeDoctorUser.Id, User = activeDoctorUser, LicenseNumber = "LIC-ACTIVE", CreatedAt = DateTime.UtcNow };
        var pendingDoctor = new Doctor { Id = Guid.NewGuid(), UserId = pendingDoctorUser.Id, User = pendingDoctorUser, LicenseNumber = "LIC-PENDING", CreatedAt = DateTime.UtcNow };

        var pendingApproval = new DoctorApprovalRequest { Id = Guid.NewGuid(), DoctorId = pendingDoctor.Id, Doctor = pendingDoctor, Status = ApprovalStatus.Pending, SubmittedAt = DateTime.UtcNow.AddHours(-1) };
        var approvedApproval = new DoctorApprovalRequest { Id = Guid.NewGuid(), DoctorId = activeDoctor.Id, Doctor = activeDoctor, Status = ApprovalStatus.Approved, SubmittedAt = DateTime.UtcNow.AddDays(-2), ReviewedAt = DateTime.UtcNow.AddDays(-1) };

        var unreadAlert = new Alert { Id = Guid.NewGuid(), PatientId = activePatient.Id, Patient = activePatient, AlertType = AlertType.HighGlucose, Severity = AlertSeverity.High, Message = "High glucose", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2) };
        var readAlert = new Alert { Id = Guid.NewGuid(), PatientId = activePatient.Id, Patient = activePatient, AlertType = AlertType.LowGlucose, Severity = AlertSeverity.Medium, Message = "Low glucose", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-2) };

        var recentAudit = new AuditLog { Id = Guid.NewGuid(), UserId = activeDoctorUser.Id, User = activeDoctorUser, Action = "Test.Recent", EntityType = "Test", CreatedAt = DateTime.UtcNow.AddDays(-10) };
        var oldAudit = new AuditLog { Id = Guid.NewGuid(), UserId = activeDoctorUser.Id, User = activeDoctorUser, Action = "Test.Old", EntityType = "Test", CreatedAt = DateTime.UtcNow.AddDays(-31) };

        db.Users.AddRange(activePatientUser, inactivePatientUser, activeDoctorUser, pendingDoctorUser);
        db.Patients.AddRange(activePatient, inactivePatient);
        db.Doctors.AddRange(activeDoctor, pendingDoctor);
        db.DoctorApprovalRequests.AddRange(pendingApproval, approvedApproval);
        db.Alerts.AddRange(unreadAlert, readAlert);
        db.AuditLogs.AddRange(recentAudit, oldAudit);
        await db.SaveChangesAsync();

        var result = await new AdminDashboardService(db).GetSummaryAsync(CancellationToken.None);

        Assert.Equal(2, result.TotalPatients);
        Assert.Equal(1, result.ActivePatients);
        Assert.Equal(2, result.TotalDoctors);
        Assert.Equal(1, result.ActiveDoctors);
        Assert.Equal(1, result.PendingDoctorApprovals);
        Assert.Equal(2, result.TotalAlerts);
        Assert.Equal(1, result.UnreadAlerts);
        Assert.Equal(1, result.TotalAuditLogsLast30Days);
    }

    private static User CreateUser(string email, Role role, AccountStatus status) => new()
    {
        Id = Guid.NewGuid(), UserName = email, Email = email, FullName = email,
        Role = role, AccountStatus = status, CreatedAt = DateTime.UtcNow
    };
}
