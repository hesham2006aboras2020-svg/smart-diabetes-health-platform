namespace Application.DTOs.Alerts;

public sealed record AlertResponse(
    Guid Id,
    Guid PatientId,
    Guid? GlucoseReadingId,
    string Type,
    string Severity,
    string Message,
    bool IsRead,
    DateTime CreatedAt);
