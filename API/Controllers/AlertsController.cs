using System.Security.Claims;
using Application.DTOs.Alerts;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize(Roles = "Patient")]
public sealed class AlertsController(IAlertService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> Get(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
        => Ok(await service.GetMyAlertsAsync(GetUserId(), unreadOnly, cancellationToken));

    [HttpPost("{alertId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid alertId, CancellationToken cancellationToken)
    {
        await service.MarkAsReadAsync(GetUserId(), alertId, cancellationToken);
        return NoContent();
    }

    [HttpPost("/api/alerts/analyze-patterns")]
    public async Task<ActionResult<int>> AnalyzePatterns(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
        => Ok(new { createdAlerts = await service.DetectRepeatedPatternsAsync(GetUserId(), days, cancellationToken) });

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}
