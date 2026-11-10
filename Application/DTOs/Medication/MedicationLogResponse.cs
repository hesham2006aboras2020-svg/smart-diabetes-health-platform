using Domain.Enums;

namespace Application.DTOs.Medication;

public sealed record MedicationLogResponse(
    Guid Id,
    DateTime ScheduledTime,
    DateTime? TakenAt,
    MedicationLogStatus Status,
    string? Notes);