using Domain.Enums;
namespace Domain.Entities;
public class HealthReport
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int NumberOfReadings { get; set; }
    public decimal? AverageGlucose { get; set; }
    public decimal? MinimumGlucose { get; set; }
    public decimal? MaximumGlucose { get; set; }
    public decimal? InRangePercentage { get; set; }
    public decimal? HighPercentage { get; set; }
    public decimal? LowPercentage { get; set; }
    public TrendType Trend { get; set; }
    public decimal? StabilityScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public Patient Patient { get; set; } = null!;
}