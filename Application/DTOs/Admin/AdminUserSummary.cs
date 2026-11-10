using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminUserSummary(
    Guid Id,
    string FullName,
    string Email,
    Role Role,
    AccountStatus AccountStatus,
    DateTime CreatedAt,
    DateTime? UpdatedAt);