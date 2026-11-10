using Domain.Enums;

namespace Application.DTOs.Medication;

public sealed record CreateMedicationLogRequest(
    DateTime ScheduledTime,
    DateTime? TakenAt,
    MedicationLogStatus Status,
    string? Notes);