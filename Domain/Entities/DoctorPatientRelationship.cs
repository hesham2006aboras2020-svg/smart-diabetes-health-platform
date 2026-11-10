using Domain.Enums;

namespace Domain.Entities;

public class DoctorPatientRelationship
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public RelationshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
