using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController(IAdminUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserSummary>>> GetUsers(
        [FromQuery] Role? role,
        [FromQuery] AccountStatus? status,
        CancellationToken cancellationToken = default)
        => Ok(await service.GetUsersAsync(role, status, cancellationToken));

    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid userId,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Status == AccountStatus.Rejected && string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "سبب الرفض مطلوب." });

        await service.UpdateStatusAsync(
            userId,
            request.Status,
            request.Reason,
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