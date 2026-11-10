using Domain.Enums;

namespace Domain.Entities;

public class DoctorApprovalRequest
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public ApprovalStatus Status { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public User? ReviewedByAdmin { get; set; }
}
