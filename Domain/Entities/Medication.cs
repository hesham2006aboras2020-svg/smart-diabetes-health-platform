namespace Domain.Entities;

public class Medication
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<MedicationSchedule> Schedules { get; set; } = new List<MedicationSchedule>();
    public ICollection<MedicationLog> Logs { get; set; } = new List<MedicationLog>();
}
