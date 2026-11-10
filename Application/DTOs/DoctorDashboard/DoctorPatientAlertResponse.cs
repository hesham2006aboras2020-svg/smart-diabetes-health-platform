using Domain.Enums;

namespace Application.DTOs.DoctorDashboard;

public sealed record DoctorPatientAlertResponse(
    Guid Id,
    AlertType Type,
    AlertSeverity Severity,
    string Title,
    string Message,
    DateTime CreatedAt,
    bool IsRead);