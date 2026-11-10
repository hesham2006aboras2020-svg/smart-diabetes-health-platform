using Application.DTOs.Alerts;

namespace Application.Interfaces;

public interface IAlertService
{
    Task<IReadOnlyList<AlertResponse>> GetMyAlertsAsync(Guid patientUserId, bool unreadOnly, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid patientUserId, Guid alertId, CancellationToken cancellationToken);
    Task<int> DetectRepeatedPatternsAsync(Guid patientUserId, int days, CancellationToken cancellationToken);
}
