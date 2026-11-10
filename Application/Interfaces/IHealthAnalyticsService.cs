using Application.DTOs.Health;

namespace Application.Interfaces;

public interface IHealthAnalyticsService
{
    Task<HealthAnalyticsResponse> AnalyzeAsync(Guid patientUserId, int days, CancellationToken cancellationToken);
}
