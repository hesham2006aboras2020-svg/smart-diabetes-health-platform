namespace Domain.Entities;

public class MedicationSchedule
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public TimeOnly Time { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }

    public Medication Medication { get; set; } = null!;
}
