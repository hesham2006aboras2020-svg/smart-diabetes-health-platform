using Application.DTOs.Notifications;

namespace Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetMyNotificationsAsync(
        Guid userId, bool unreadOnly, CancellationToken cancellationToken);

    Task MarkAsReadAsync(
        Guid userId, Guid notificationId, CancellationToken cancellationToken);

    Task MarkAllAsReadAsync(
        Guid userId, CancellationToken cancellationToken);

    Task RegisterDeviceTokenAsync(
        Guid userId, RegisterDeviceTokenRequest request, CancellationToken cancellationToken);
}