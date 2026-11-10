using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminAuditLogService
{
    Task<IReadOnlyList<AuditLogResponse>> GetAsync(
        DateTime? from,
        DateTime? to,
        string? action,
        CancellationToken cancellationToken);
}