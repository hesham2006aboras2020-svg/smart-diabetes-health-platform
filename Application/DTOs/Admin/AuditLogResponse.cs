namespace Application.DTOs.Admin;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? UserId,
    string? UserName,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Description,
    DateTime CreatedAt,
    string? IpAddress);