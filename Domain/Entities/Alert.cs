using Domain.Enums;
namespace Domain.Entities;
public class Alert
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? GlucoseReadingId { get; set; }
    public AlertType AlertType { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Patient Patient { get; set; } = null!;
    public GlucoseReading? GlucoseReading { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}