using Domain.Enums;

namespace Domain.Entities;

public class GlucoseReading
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public decimal GlucoseValue { get; set; }
    public GlucoseUnit Unit { get; set; }
    public DateTime MeasurementDateTime { get; set; }
    public MeasurementContext MeasurementContext { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
