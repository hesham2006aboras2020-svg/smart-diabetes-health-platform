using Domain.Enums;

namespace Domain.Entities;

public class MedicationLog
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? TakenAt { get; set; }
    public MedicationLogStatus Status { get; set; }
    public string? Notes { get; set; }

    public Medication Medication { get; set; } = null!;
}
