namespace Application.DTOs.HealthReport;

public sealed record HealthReportResponse(
    Guid Id,
    Guid PatientId,
    DateOnly StartDate,
    DateOnly EndDate,
    int NumberOfReadings,
    decimal? AverageGlucose,
    decimal? MinimumGlucose,
    decimal? MaximumGlucose,
    decimal? InRangePercentage,
    decimal? HighPercentage,
    decimal? LowPercentage,
    string Trend,
    decimal? StabilityScore,
    DateTime CreatedAt);