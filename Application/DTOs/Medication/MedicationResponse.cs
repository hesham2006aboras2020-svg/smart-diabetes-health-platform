namespace Application.DTOs.Medication;

public sealed record MedicationResponse(
    Guid Id,
    string Name,
    string? Dosage,
    string? Instructions,
    bool IsActive,
    IReadOnlyList<MedicationScheduleResponse> Schedules);