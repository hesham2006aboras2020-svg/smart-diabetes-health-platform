namespace Application.DTOs.Health;

public sealed record HealthAnalyticsResponse(
    int Days,
    int ReadingCount,
    decimal? AverageMgDl,
    decimal? MinimumMgDl,
    decimal? MaximumMgDl,
    decimal? InRangePercentage,
    decimal? HighPercentage,
    decimal? LowPercentage,
    string Trend,
    decimal? StabilityScore,
    string StabilityLevel,
    string FollowUpPriority);
