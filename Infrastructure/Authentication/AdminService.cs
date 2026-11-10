using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class AdminService(DiabetesDbContext db) : IAdminService
{
    public async Task<IReadOnlyList<DoctorApprovalSummary>> GetPendingDoctorApprovalsAsync(CancellationToken cancellationToken)
    {
        return await db.DoctorApprovalRequests
            .AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderBy(x => x.SubmittedAt)
            .Select(x => new DoctorApprovalSummary(
                x.Id,
                x.DoctorId,
                x.Doctor.UserId,
                x.Doctor.User.FullName,
                x.Doctor.User.Email!,
                x.Doctor.LicenseNumber,
                x.Doctor.Specialization,
                x.Doctor.ClinicName,
                x.Status.ToString(),
                x.SubmittedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task ReviewDoctorApprovalAsync(
        Guid approvalRequestId,
        bool approve,
        ReviewDoctorApprovalRequest request,
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        var approval = await db.DoctorApprovalRequests
            .Include(x => x.Doctor)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == approvalRequestId, cancellationToken);

        if (approval is null)
            throw new KeyNotFoundException("Doctor approval request was not found.");

        if (approval.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("This approval request has already been reviewed.");

        var now = DateTime.UtcNow;
        approval.Status = approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        approval.ReviewedByAdminId = adminUserId;
        approval.ReviewedAt = now;
        approval.AdminNotes = request.AdminNotes?.Trim();

        if (approve)
        {
            approval.RejectionReason = null;
            approval.Doctor.User.AccountStatus = AccountStatus.Active;
            approval.Doctor.User.UpdatedAt = now;
        }
        else
        {
            approval.RejectionReason = string.IsNullOrWhiteSpace(request.RejectionReason)
                ? "Doctor registration request was rejected by administration."
                : request.RejectionReason.Trim();

            approval.Doctor.User.AccountStatus = AccountStatus.Rejected;
            approval.Doctor.User.UpdatedAt = now;
        }

        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            Action = approve ? "DoctorApprovalApproved" : "DoctorApprovalRejected",
            EntityType = nameof(DoctorApprovalRequest),
            EntityId = approval.Id,
            Description = approve
                ? $"Doctor account {approval.Doctor.User.Email} was approved."
                : $"Doctor account {approval.Doctor.User.Email} was rejected. Reason: {approval.RejectionReason}",
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
