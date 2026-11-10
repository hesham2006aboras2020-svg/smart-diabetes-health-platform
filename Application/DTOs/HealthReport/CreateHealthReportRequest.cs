namespace Application.DTOs.HealthReport;

public sealed record CreateHealthReportRequest(
    DateOnly StartDate,
    DateOnly EndDate);