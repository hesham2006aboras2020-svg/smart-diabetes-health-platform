using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class AdminUserService(DiabetesDbContext db) : IAdminUserService
{
    public async Task<IReadOnlyList<AdminUserSummary>> GetUsersAsync(
        Role? role,
        AccountStatus? status,
        CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().AsQueryable();

        if (role.HasValue)
            query = query.Where(x => x.Role == role.Value);

        if (status.HasValue)
            query = query.Where(x => x.AccountStatus == status.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .Select(x => new AdminUserSummary(
                x.Id,
                x.FullName,
                x.Email!,
                x.Role,
                x.AccountStatus,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        Guid userId,
        AccountStatus status,
        string? reason,
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        if (userId == adminUserId)
            throw new InvalidOperationException("لا يمكن للمشرف تغيير حالة حسابه من خلال هذه العملية.");

        if (status == AccountStatus.Pending)
            throw new ArgumentException("لا يمكن تعيين حالة الحساب إلى Pending من خلال لوحة الإدارة.");

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("المستخدم غير موجود.");

        if (user.Role == Role.Admin && status != AccountStatus.Active)
            throw new InvalidOperationException("لا يمكن تعليق أو رفض حساب مشرف من خلال هذه العملية.");

        var oldStatus = user.AccountStatus;
        user.AccountStatus = status;
        user.UpdatedAt = DateTime.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            Action = "UserAccountStatusChanged",
            EntityType = nameof(User),
            EntityId = user.Id,
            Description = $"Account status changed from {oldStatus} to {status}. Reason: {reason?.Trim()}",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}