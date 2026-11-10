using Application.DTOs.HealthReport;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/health-reports")]
[Authorize(Roles = "Patient")]
public sealed class HealthReportsController(IHealthReportService healthReportService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<HealthReportResponse>> Create([FromBody] CreateHealthReportRequest request, CancellationToken cancellationToken)
        => Ok(await healthReportService.CreateAsync(GetUserId(), request, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HealthReportResponse>>> GetAll(CancellationToken cancellationToken)
        => Ok(await healthReportService.GetAllAsync(GetUserId(), cancellationToken));

    [HttpGet("latest")]
    public async Task<ActionResult<HealthReportResponse>> GetLatest(CancellationToken cancellationToken)
    {
        var response = await healthReportService.GetLatestAsync(GetUserId(), cancellationToken);
        return response is null ? NotFound(new { message = "لا يوجد تقرير صحي حتى الآن." }) : Ok(response);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("معرّف المستخدم غير صالح.");
    }
}