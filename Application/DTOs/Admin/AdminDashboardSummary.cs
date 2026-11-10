namespace Application.DTOs.Admin;

public sealed record AdminDashboardSummary(
    int TotalPatients,
    int ActivePatients,
    int TotalDoctors,
    int ActiveDoctors,
    int PendingDoctorApprovals,
    int TotalAlerts,
    int UnreadAlerts,
    int TotalAuditLogsLast30Days);