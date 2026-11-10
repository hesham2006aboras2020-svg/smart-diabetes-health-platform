using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public sealed class AdminDashboardController(IAdminDashboardService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<AdminDashboardSummary>> GetSummary(
        CancellationToken cancellationToken = default)
        => Ok(await service.GetSummaryAsync(cancellationToken));
}