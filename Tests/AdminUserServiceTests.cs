using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class AdminUserServiceTests
{
    [Fact]
    public async Task UpdateStatus_CannotChangeOwnAccount()
    {
        await using var db = CreateDbContext();
        var admin = CreateUser("admin@example.com", Role.Admin, AccountStatus.Active);
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var service = new AdminUserService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(
                admin.Id,
                AccountStatus.Suspended,
                "Self change",
                admin.Id,
                CancellationToken.None));

        Assert.Equal(AccountStatus.Active, (await db.Users.SingleAsync()).AccountStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task UpdateStatus_CannotSuspendAnotherAdmin()
    {
        await using var db = CreateDbContext();
        var actingAdmin = CreateUser("admin1@example.com", Role.Admin, AccountStatus.Active);
        var targetAdmin = CreateUser("admin2@example.com", Role.Admin, AccountStatus.Active);
        db.Users.AddRange(actingAdmin, targetAdmin);
        await db.SaveChangesAsync();

        var service = new AdminUserService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(
                targetAdmin.Id,
                AccountStatus.Suspended,
                "Security test",
                actingAdmin.Id,
                CancellationToken.None));

        Assert.Equal(AccountStatus.Active, (await db.Users.SingleAsync(x => x.Id == targetAdmin.Id)).AccountStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task UpdateStatus_ChangesUserAndWritesAuditLog()
    {
        await using var db = CreateDbContext();
        var admin = CreateUser("admin@example.com", Role.Admin, AccountStatus.Active);
        var patient = CreateUser("patient@example.com", Role.Patient, AccountStatus.Active);
        db.Users.AddRange(admin, patient);
        await db.SaveChangesAsync();

        var service = new AdminUserService(db);

        await service.UpdateStatusAsync(
            patient.Id,
            AccountStatus.Suspended,
            "Repeated invalid login attempts.",
            admin.Id,
            CancellationToken.None);

        var savedPatient = await db.Users.SingleAsync(x => x.Id == patient.Id);
        Assert.Equal(AccountStatus.Suspended, savedPatient.AccountStatus);

        var audit = await db.AuditLogs.SingleAsync();
        Assert.Equal("UserAccountStatusChanged", audit.Action);
        Assert.Equal(patient.Id, audit.EntityId);
        Assert.Equal(admin.Id, audit.UserId);
        Assert.Contains("Active to Suspended", audit.Description);
        Assert.Contains("Repeated invalid login attempts.", audit.Description);
    }

    [Fact]
    public async Task UpdateStatus_RejectsPendingStatusChange()
    {
        await using var db = CreateDbContext();
        var admin = CreateUser("admin@example.com", Role.Admin, AccountStatus.Active);
        var patient = CreateUser("patient2@example.com", Role.Patient, AccountStatus.Active);
        db.Users.AddRange(admin, patient);
        await db.SaveChangesAsync();

        var service = new AdminUserService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateStatusAsync(
                patient.Id,
                AccountStatus.Pending,
                null,
                admin.Id,
                CancellationToken.None));

        Assert.Equal(AccountStatus.Active, (await db.Users.SingleAsync(x => x.Id == patient.Id)).AccountStatus);
        Assert.Empty(await db.AuditLogs.ToListAsync());
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
