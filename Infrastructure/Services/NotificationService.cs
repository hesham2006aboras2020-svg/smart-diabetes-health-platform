using Application.DTOs.Notifications;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class NotificationService(DiabetesDbContext db) : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponse>> GetMyNotificationsAsync(
        Guid userId, bool unreadOnly, CancellationToken cancellationToken)
    {
        var query = db.Notifications.AsNoTracking()
            .Where(x => x.UserId == userId);

        if (unreadOnly)
            query = query.Where(x => x.ReadAt == null);

        return await query
            .OrderByDescending(x => x.SentAt ?? DateTime.MinValue)
            .ThenByDescending(x => x.Id)
            .Take(200)
            .Select(x => new NotificationResponse(
                x.Id,
                x.AlertId,
                x.Title,
                x.Body,
                x.Type,
                x.ReadAt != null,
                x.IsSent,
                x.SentAt,
                x.Alert != null ? x.Alert.CreatedAt : DateTime.MinValue))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken);

        if (notification is null)
            throw new KeyNotFoundException("الإشعار غير موجود.");

        notification.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterDeviceTokenAsync(
        Guid userId,
        RegisterDeviceTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("رمز الجهاز مطلوب.");

        var token = request.Token.Trim();

        if (token.Length > 500)
            throw new ArgumentException("رمز الجهاز طويل جدًا.");

        var existing = await db.DeviceTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (existing is not null)
        {
            existing.UserId = userId;
            existing.DeviceType = string.IsNullOrWhiteSpace(request.DeviceType)
                ? existing.DeviceType
                : request.DeviceType.Trim();
            existing.LastUsedAt = DateTime.UtcNow;
        }
        else
        {
            db.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                DeviceType = string.IsNullOrWhiteSpace(request.DeviceType)
                    ? null
                    : request.DeviceType.Trim(),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}