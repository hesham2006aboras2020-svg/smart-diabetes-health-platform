namespace Application.DTOs.Admin;

public sealed record ReviewDoctorApprovalRequest(
    string? RejectionReason,
    string? AdminNotes);
