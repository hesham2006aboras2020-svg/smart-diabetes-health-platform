using Application.DTOs.Glucose;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Patient")]
[Route("api/glucose")]
public class GlucoseController(IGlucoseService glucoseService) : ControllerBase
{
    [HttpPost("readings")]
    public async Task<ActionResult<GlucoseReadingResponse>> AddReading(
        CreateGlucoseReadingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await glucoseService.AddReadingAsync(GetUserId(), request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("readings")]
    public async Task<ActionResult<IReadOnlyList<GlucoseReadingResponse>>> GetReadings(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await glucoseService.GetReadingsAsync(GetUserId(), from, to, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GlucoseSummaryResponse>> GetSummary(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await glucoseService.GetSummaryAsync(GetUserId(), days, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("جلسة المستخدم غير صالحة.");

        return userId;
    }
}