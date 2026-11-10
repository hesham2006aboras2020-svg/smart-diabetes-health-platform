using Domain.Enums;

namespace Application.DTOs.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid? AlertId,
    string Title,
    string Body,
    NotificationType Type,
    bool IsRead,
    bool IsSent,
    DateTime? SentAt,
    DateTime CreatedAt);