namespace Application.DTOs.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string Role,
    string? LicenseNumber,
    string? Specialization,
    string? ClinicName);
