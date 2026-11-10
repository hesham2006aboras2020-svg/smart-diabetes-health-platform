namespace Application.DTOs.Medication;

public sealed record MedicationScheduleResponse(
    Guid Id,
    TimeOnly Time,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive);