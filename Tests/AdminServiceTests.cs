using Application.DTOs.Admin;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class AdminServiceTests
{
    [Fact]
    public async Task ApprovePendingDoctor_ActivatesDoctorAndWritesAuditLog()
    {
        await using var db = CreateDbContext();
        var adminId = Guid.NewGuid();
        var doctorUser = CreateUser("doctor@example.com", Role.Doctor, AccountStatus.Pending);
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            LicenseNumber = "LIC-001",
            Specialization = "Endocrinology",
            User = doctorUser
        };
        var approval = new DoctorApprovalRequest
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = ApprovalStatus.Pending,
            SubmittedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        db.Users.AddRange(CreateUser("admin@example.com", Role.Admin, AccountStatus.Active), doctorUser);
        db.Doctors.Add(doctor);
        db.DoctorApprovalRequests.Add(approval);
        await db.SaveChangesAsync();

        var service = new AdminService(db);
        await service.ReviewDoctorApprovalAsync(
            approval.Id,
            true,
            new ReviewDoctorApprovalRequest(null, "Approved after verification."),
            adminId,
            CancellationToken.None);

        var saved = await db.DoctorApprovalRequests.Include(x => x.Doctor).ThenInclude(x => x.User).SingleAsync();
        Assert.Equal(ApprovalStatus.Approved, saved.Status);
        Assert.Equal(AccountStatus.Active, saved.Doctor.User.AccountStatus);
        Assert.Null(saved.RejectionReason);
        Assert.Equal(adminId, saved.ReviewedByAdminId);
        Assert.NotNull(saved.ReviewedAt);

        var audit = await db.AuditLogs.SingleAsync();
        Assert.Equal("DoctorApprovalApproved", audit.Action);
        Assert.Equal(approval.Id, audit.EntityId);
        Assert.Equal(adminId, audit.UserId);
    }

    [Fact]
    public async Task RejectPendingDoctor_SetsRejectedStatusAndDefaultReason()
    {
        await using var db = CreateDbContext();
        var adminId = Guid.NewGuid();
        var doctorUser = CreateUser("doctor2@example.com", Role.Doctor, AccountStatus.Pending);
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            LicenseNumber = "LIC-002",
            User = doctorUser
        };
        var approval = new DoctorApprovalRequest
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = ApprovalStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        db.Users.Add(doctorUser);
        db.Doctors.Add(doctor);
        db.DoctorApprovalRequests.Add(approval);
        await db.SaveChangesAsync();

        var service = new AdminService(db);
        await service.ReviewDoctorApprovalAsync(
            approval.Id,
            false,
            new ReviewDoctorApprovalRequest("   ", "Incomplete documentation."),
            adminId,
            CancellationToken.None);

        var saved = await db.DoctorApprovalRequests.Include(x => x.Doctor).ThenInclude(x => x.User).SingleAsync();
        Assert.Equal(ApprovalStatus.Rejected, saved.Status);
        Assert.Equal(AccountStatus.Rejected, saved.Doctor.User.AccountStatus);
        Assert.Equal("Doctor registration request was rejected by administration.", saved.RejectionReason);
        Assert.Equal("Incomplete documentation.", saved.AdminNotes);

        var audit = await db.AuditLogs.SingleAsync();
        Assert.Equal("DoctorApprovalRejected", audit.Action);
        Assert.Contains(saved.RejectionReason!, audit.Description);
    }

    [Fact]
    public async Task ReviewAlreadyReviewedApproval_ThrowsAndDoesNotCreateAnotherAuditLog()
    {
        await using var db = CreateDbContext();
        var doctorUser = CreateUser("reviewed@example.com", Role.Doctor, AccountStatus.Active);
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            LicenseNumber = "LIC-006",
            User = doctorUser
        };
        var approval = new DoctorApprovalRequest
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = ApprovalStatus.Approved,
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            ReviewedAt = DateTime.UtcNow
        };
        db.Users.Add(doctorUser);
        db.Doctors.Add(doctor);
        db.DoctorApprovalRequests.Add(approval);
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReviewDoctorApprovalAsync(
                approval.Id,
                true,
                new ReviewDoctorApprovalRequest(null, null),
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task GetPendingDoctorApprovals_ReturnsOnlyPendingRequestsOrderedBySubmissionTime()
    {
        await using var db = CreateDbContext();
        var older = AddDoctorApproval(db, "older@example.com", "LIC-003", DateTime.UtcNow.AddMinutes(-20));
        var newer = AddDoctorApproval(db, "newer@example.com", "LIC-004", DateTime.UtcNow.AddMinutes(-10));
        var reviewed = AddDoctorApproval(db, "reviewed@example.com", "LIC-005", DateTime.UtcNow.AddMinutes(-30));
        reviewed.Status = ApprovalStatus.Approved;
        await db.SaveChangesAsync();

        var service = new AdminService(db);
        var result = await service.GetPendingDoctorApprovalsAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(older.DoctorId, result[0].DoctorId);
        Assert.Equal(newer.DoctorId, result[1].DoctorId);
        Assert.All(result, item => Assert.Equal(nameof(ApprovalStatus.Pending), item.Status));
    }

    private static DoctorApprovalRequest AddDoctorApproval(
        DiabetesDbContext db,
        string email,
        string licenseNumber,
        DateTime submittedAt)
    {
        var user = CreateUser(email, Role.Doctor, AccountStatus.Pending);
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LicenseNumber = licenseNumber,
            Specialization = "Internal Medicine",
            ClinicName = "Clinic",
            User = user
        };
        var approval = new DoctorApprovalRequest
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = ApprovalStatus.Pending,
            SubmittedAt = submittedAt
        };
        db.Users.Add(user);
        db.Doctors.Add(doctor);
        db.DoctorApprovalRequests.Add(approval);
        return approval;
    }

    private static User CreateUser(string email, Role role, AccountStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        FullName = email.Split('@')[0],
        Role = role,
        AccountStatus = status,
        CreatedAt = DateTime.UtcNow
    };

    private static DiabetesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DiabetesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DiabetesDbContext(options);
    }
}
