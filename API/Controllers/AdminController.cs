using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("doctor-approvals/pending")]
    public async Task<ActionResult<IReadOnlyList<DoctorApprovalSummary>>> GetPendingDoctorApprovals(CancellationToken cancellationToken)
        => Ok(await adminService.GetPendingDoctorApprovalsAsync(cancellationToken));

    [HttpPost("doctor-approvals/{approvalRequestId:guid}/approve")]
    public async Task<IActionResult> ApproveDoctor(
        Guid approvalRequestId,
        [FromBody] ReviewDoctorApprovalRequest request,
        CancellationToken cancellationToken)
    {
        await adminService.ReviewDoctorApprovalAsync(
            approvalRequestId,
            approve: true,
            request,
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("doctor-approvals/{approvalRequestId:guid}/reject")]
    public async Task<IActionResult> RejectDoctor(
        Guid approvalRequestId,
        [FromBody] ReviewDoctorApprovalRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return BadRequest(new { message = "Rejection reason is required." });

        await adminService.ReviewDoctorApprovalAsync(
            approvalRequestId,
            approve: false,
            request,
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}
