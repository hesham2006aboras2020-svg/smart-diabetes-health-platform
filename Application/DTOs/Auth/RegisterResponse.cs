namespace Application.DTOs.Auth;

public sealed record RegisterResponse(
    string Message,
    string Role,
    string AccountStatus,
    bool RequiresApproval,
    AuthResponse? Authentication);
