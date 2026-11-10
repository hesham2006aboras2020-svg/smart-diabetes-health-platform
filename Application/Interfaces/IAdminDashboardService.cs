using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummary> GetSummaryAsync(CancellationToken cancellationToken);
}