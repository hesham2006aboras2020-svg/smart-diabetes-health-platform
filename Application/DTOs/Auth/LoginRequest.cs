namespace Application.DTOs.Auth;

public sealed record LoginRequest(string PhoneNumber, string Password);