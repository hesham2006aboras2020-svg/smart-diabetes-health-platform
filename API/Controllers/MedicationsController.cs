using Application.DTOs.Medication;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Patient")]
[Route("api/medications")]
public class MedicationsController(IMedicationService medicationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MedicationResponse>> Create(
        CreateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await medicationService.CreateMedicationAsync(GetUserId(), request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicationResponse>>> GetAll(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await medicationService.GetMedicationsAsync(GetUserId(), activeOnly, cancellationToken));
    }

    [HttpPatch("{medicationId:guid}/status")]
    public async Task<IActionResult> SetStatus(
        Guid medicationId,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            await medicationService.SetMedicationStatusAsync(GetUserId(), medicationId, isActive, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{medicationId:guid}/schedules")]
    public async Task<ActionResult<MedicationScheduleResponse>> AddSchedule(
        Guid medicationId,
        CreateMedicationScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await medicationService.AddScheduleAsync(
                GetUserId(), medicationId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{medicationId:guid}/logs")]
    public async Task<ActionResult<MedicationLogResponse>> AddLog(
        Guid medicationId,
        CreateMedicationLogRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await medicationService.AddLogAsync(
                GetUserId(), medicationId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{medicationId:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<MedicationLogResponse>>> GetLogs(
        Guid medicationId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await medicationService.GetLogsAsync(
                GetUserId(), medicationId, from, to, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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