namespace Application.DTOs.DoctorDashboard;

public sealed record DoctorPatientMedicationAdherenceResponse(
    Guid PatientId,
    int Days,
    int TotalDosesLogged,
    int TakenDoses,
    int MissedDoses,
    int SkippedDoses,
    decimal AdherencePercentage,
    int ActiveMedicationCount,
    DateTime From,
    DateTime To);
