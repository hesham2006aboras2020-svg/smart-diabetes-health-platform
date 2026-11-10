using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class AdminDashboardService(DiabetesDbContext db) : IAdminDashboardService
{
    public async Task<AdminDashboardSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddDays(-30);

        var totalPatients = await db.Patients.CountAsync(cancellationToken);
        var activePatients = await db.Patients.CountAsync(
            x => x.User.AccountStatus == AccountStatus.Active, cancellationToken);

        var totalDoctors = await db.Doctors.CountAsync(cancellationToken);
        var activeDoctors = await db.Doctors.CountAsync(
            x => x.User.AccountStatus == AccountStatus.Active, cancellationToken);

        var pendingApprovals = await db.DoctorApprovalRequests.CountAsync(
            x => x.Status == ApprovalStatus.Pending, cancellationToken);

        var totalAlerts = await db.Alerts.CountAsync(cancellationToken);
        var unreadAlerts = await db.Alerts.CountAsync(
            x => !x.IsRead, cancellationToken);

        var auditLogsLast30Days = await db.AuditLogs.CountAsync(
            x => x.CreatedAt >= since, cancellationToken);

        return new AdminDashboardSummary(
            totalPatients,
            activePatients,
            totalDoctors,
            activeDoctors,
            pendingApprovals,
            totalAlerts,
            unreadAlerts,
            auditLogsLast30Days);
    }
}