namespace Application.DTOs.Glucose;

public sealed record GlucoseSummaryResponse(
    int NumberOfReadings,
    decimal? AverageGlucoseMgDl,
    decimal? MinimumGlucoseMgDl,
    decimal? MaximumGlucoseMgDl,
    decimal? InRangePercentage,
    decimal? HighPercentage,
    decimal? LowPercentage,
    string Trend);