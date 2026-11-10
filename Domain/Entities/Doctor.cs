namespace Domain.Entities;

public class Doctor
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? ClinicName { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<DoctorApprovalRequest> ApprovalRequests { get; set; } = new List<DoctorApprovalRequest>();
    public ICollection<DoctorPatientRelationship> PatientRelationships { get; set; } = new List<DoctorPatientRelationship>();
    public ICollection<DoctorNote> Notes { get; set; } = new List<DoctorNote>();
}
