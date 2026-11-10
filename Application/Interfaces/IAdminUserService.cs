using Application.DTOs.Admin;
using Domain.Enums;

namespace Application.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserSummary>> GetUsersAsync(
        Role? role,
        AccountStatus? status,
        CancellationToken cancellationToken);

    Task UpdateStatusAsync(
        Guid userId,
        AccountStatus status,
        string? reason,
        Guid adminUserId,
        CancellationToken cancellationToken);
}