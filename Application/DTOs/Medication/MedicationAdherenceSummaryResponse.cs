namespace Application.DTOs.Medication;

public sealed record MedicationAdherenceSummaryResponse(
    int Days,
    int TotalDosesLogged,
    int TakenDoses,
    int MissedDoses,
    int SkippedDoses,
    decimal AdherencePercentage,
    int ActiveMedicationCount,
    DateTime From,
    DateTime To);