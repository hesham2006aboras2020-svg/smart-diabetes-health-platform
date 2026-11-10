namespace Domain.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<DoctorPatientRelationship> DoctorRelationships { get; set; } = new List<DoctorPatientRelationship>();
    public ICollection<GlucoseReading> GlucoseReadings { get; set; } = new List<GlucoseReading>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();
    public ICollection<HealthReport> HealthReports { get; set; } = new List<HealthReport>();
}
