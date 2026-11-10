using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public sealed class AdminAuditLogsController(IAdminAuditLogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogResponse>>> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? action,
        CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new { message = "تاريخ البداية يجب أن يسبق تاريخ النهاية." });

        return Ok(await service.GetAsync(from, to, action, cancellationToken));
    }
}