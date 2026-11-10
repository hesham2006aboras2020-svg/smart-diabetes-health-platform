using Application.DTOs.Admin;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class AdminAuditLogService(DiabetesDbContext db) : IAdminAuditLogService
{
    public async Task<IReadOnlyList<AuditLogResponse>> GetAsync(
        DateTime? from,
        DateTime? to,
        string? action,
        CancellationToken cancellationToken)
    {
        var query = db.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action.Trim());

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .Select(x => new AuditLogResponse(
                x.Id,
                x.UserId,
                x.User == null ? null : x.User.UserName,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.Description,
                x.CreatedAt,
                x.IpAddress))
            .ToListAsync(cancellationToken);
    }
}