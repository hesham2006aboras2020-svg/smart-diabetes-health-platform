namespace Application.DTOs.Notifications;

public sealed record RegisterDeviceTokenRequest(
    string Token,
    string? DeviceType);