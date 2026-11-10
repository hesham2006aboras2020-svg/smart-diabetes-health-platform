using Application.DTOs.Glucose;

namespace Application.Interfaces;

public interface IGlucoseService
{
    Task<GlucoseReadingResponse> AddReadingAsync(
        Guid userId,
        CreateGlucoseReadingRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseReadingResponse>> GetReadingsAsync(
        Guid userId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<GlucoseSummaryResponse> GetSummaryAsync(
        Guid userId,
        int days,
        CancellationToken cancellationToken = default);
}