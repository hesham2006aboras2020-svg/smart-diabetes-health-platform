using System.Security.Claims;
using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/doctor-patients")]
[Authorize(Roles = "Doctor")]
public sealed class DoctorPatientAlertsController(IDoctorPatientAlertsService service) : ControllerBase
{
    [HttpGet("{patientId:guid}/alerts")]
    public async Task<ActionResult<IReadOnlyList<DoctorPatientAlertResponse>>> GetAlerts(
        Guid patientId, [FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
        => Ok(await service.GetAlertsAsync(GetUserId(), patientId, unreadOnly, cancellationToken));

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}