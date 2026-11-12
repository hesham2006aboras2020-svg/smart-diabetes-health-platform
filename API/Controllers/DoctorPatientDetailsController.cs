using System.Security.Claims;
using Application.DTOs.DoctorDashboard;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/doctor-patients")]
[Authorize(Roles = "Doctor")]
public sealed class DoctorPatientDetailsController(IDoctorPatientDetailsService service) : ControllerBase
{
    [HttpGet("{patientId:guid}")]
    public async Task<ActionResult<DoctorPatientDetailsResponse>> GetDetails(
        Guid patientId, CancellationToken cancellationToken = default)
        => Ok(await service.GetDetailsAsync(GetUserId(), patientId, cancellationToken));

    [HttpGet("{patientId:guid}/medication-adherence")]
    public async Task<ActionResult<DoctorPatientMedicationAdherenceResponse>> GetMedicationAdherence(
        Guid patientId, [FromQuery] int days = 7, CancellationToken cancellationToken = default)
        => Ok(await service.GetMedicationAdherenceAsync(GetUserId(), patientId, days, cancellationToken));

    [HttpGet("{patientId:guid}/readings")]
    public async Task<ActionResult<IReadOnlyList<DoctorPatientReadingResponse>>> GetReadings(
        Guid patientId, [FromQuery] int days = 7, CancellationToken cancellationToken = default)
        => Ok(await service.GetReadingsAsync(GetUserId(), patientId, days, cancellationToken));

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}