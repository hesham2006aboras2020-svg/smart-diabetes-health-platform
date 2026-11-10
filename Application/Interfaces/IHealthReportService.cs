using Application.DTOs.HealthReport;

namespace Application.Interfaces;

public interface IHealthReportService
{
    Task<HealthReportResponse> CreateAsync(Guid patientUserId, CreateHealthReportRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<HealthReportResponse>> GetAllAsync(Guid patientUserId, CancellationToken cancellationToken);
    Task<HealthReportResponse?> GetLatestAsync(Guid patientUserId, CancellationToken cancellationToken);
}