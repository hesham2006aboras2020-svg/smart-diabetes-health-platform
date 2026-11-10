using System.Security.Claims;
using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/doctor-dashboard")]
[Authorize(Roles = "Doctor")]
public sealed class DoctorDashboardController(IDoctorDashboardService service) : ControllerBase
{
    [HttpGet("patients")]
    public async Task<ActionResult<IReadOnlyList<DoctorPatientDashboardResponse>>> GetPatients(
        CancellationToken cancellationToken)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var doctorUserId))
            throw new UnauthorizedAccessException("Invalid user identity.");

        return Ok(await service.GetPatientsAsync(doctorUserId, cancellationToken));
    }
}
