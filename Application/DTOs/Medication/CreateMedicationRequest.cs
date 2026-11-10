namespace Application.DTOs.Medication;

public sealed record CreateMedicationRequest(
    string Name,
    string? Dosage,
    string? Instructions);