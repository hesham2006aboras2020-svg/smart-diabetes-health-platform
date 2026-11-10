using System.Security.Claims;
using Application.DTOs.Health;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/health-analytics")]
[Authorize(Roles = "Patient")]
public sealed class HealthAnalyticsController(IHealthAnalyticsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HealthAnalyticsResponse>> Analyze(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");

        return Ok(await service.AnalyzeAsync(userId, days, cancellationToken));
    }
}
