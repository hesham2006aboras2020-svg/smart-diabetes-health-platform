namespace Application.DTOs.Patient;

public sealed record UpdatePatientProfileRequest(
    string FullName,
    DateOnly? DateOfBirth,
    string? Gender);