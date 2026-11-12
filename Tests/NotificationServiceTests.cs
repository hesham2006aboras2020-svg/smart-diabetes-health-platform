using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task MarkAllAsReadAsync_MarksOnlyCurrentUserUnreadNotifications()
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

        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "other@test.local",
            Email = "other@test.local",
            FullName = "Other Patient",
            Role = Role.Patient,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var ownUnread = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Title = "تنبيه",
            Body = "قراءة مرتفعة",
            Type = NotificationType.Alert,
            CreatedAt = DateTime.UtcNow
        };

        var ownRead = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Title = "تنبيه سابق",
            Body = "تمت القراءة",
            Type = NotificationType.Alert,
            ReadAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };

        var otherUnread = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = otherUser.Id,
            User = otherUser,
            Title = "تنبيه آخر",
            Body = "لا يخص المستخدم الحالي",
            Type = NotificationType.Alert,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(user, otherUser);
        db.Notifications.AddRange(ownUnread, ownRead, otherUnread);
        await db.SaveChangesAsync();

        var service = new NotificationService(db);

        await service.MarkAllAsReadAsync(user.Id, CancellationToken.None);

        var updatedOwnUnread = await db.Notifications.FindAsync(ownUnread.Id);
        var updatedOwnRead = await db.Notifications.FindAsync(ownRead.Id);
        var updatedOtherUnread = await db.Notifications.FindAsync(otherUnread.Id);

        Assert.NotNull(updatedOwnUnread?.ReadAt);
        Assert.NotNull(updatedOwnRead?.ReadAt);
        Assert.Null(updatedOtherUnread?.ReadAt);
    }
}
