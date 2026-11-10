namespace Application.DTOs.Admin;

public sealed record DoctorApprovalSummary(
    Guid ApprovalRequestId,
    Guid DoctorId,
    Guid UserId,
    string FullName,
    string Email,
    string LicenseNumber,
    string? Specialization,
    string? ClinicName,
    string Status,
    DateTime SubmittedAt);
