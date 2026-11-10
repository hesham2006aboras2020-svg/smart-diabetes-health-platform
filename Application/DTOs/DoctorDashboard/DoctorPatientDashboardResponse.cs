namespace Application.DTOs.DoctorDashboard;

public sealed record DoctorPatientDashboardResponse(
    Guid PatientId,
    string PatientName,
    DateTime? LastReadingAt,
    decimal? LastReadingMgDl,
    decimal? AverageMgDl,
    decimal? InRangePercentage,
    string Trend,
    decimal? StabilityScore,
    string StabilityLevel,
    string FollowUpPriority,
    int UnreadAlertCount);
