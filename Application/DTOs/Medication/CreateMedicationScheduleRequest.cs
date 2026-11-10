namespace Application.DTOs.Medication;

public sealed record CreateMedicationScheduleRequest(
    TimeOnly Time,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate);