namespace Application.DTOs.Patient;

public sealed record PatientProfileResponse(
    Guid PatientId,
    string FullName,
    string Email,
    DateOnly? DateOfBirth,
    string? Gender,
    string AccountStatus,
    DateTime CreatedAt);