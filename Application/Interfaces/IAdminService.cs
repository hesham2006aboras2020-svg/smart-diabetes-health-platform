using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<DoctorApprovalSummary>> GetPendingDoctorApprovalsAsync(CancellationToken cancellationToken);
    Task ReviewDoctorApprovalAsync(Guid approvalRequestId, bool approve, ReviewDoctorApprovalRequest request, Guid adminUserId, CancellationToken cancellationToken);
}
