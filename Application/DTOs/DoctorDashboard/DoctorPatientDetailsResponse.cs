namespace Application.DTOs.DoctorDashboard;

public sealed record DoctorPatientDetailsResponse(
    Guid PatientId,
    string PatientName,
    string Email,
    DateOnly? DateOfBirth,
    string? Gender,
    DateTime? LastReadingAt,
    decimal? LastReadingMgDl,
    decimal? AverageMgDl7Days,
    decimal? InRangePercentage7Days,
    string Trend7Days,
    int UnreadAlertCount,
    int ActiveMedicationCount);