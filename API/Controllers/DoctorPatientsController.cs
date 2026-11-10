using System.Security.Claims;
using Application.DTOs.DoctorPatient;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/doctor-patients")]
public sealed class DoctorPatientsController(IDoctorPatientService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<DoctorPatientRelationshipResponse>> AddPatient(
        [FromBody] CreateDoctorPatientRelationshipRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.AddPatientAsync(GetCurrentUserId(), request, cancellationToken));

    [HttpGet("doctor")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<IReadOnlyList<DoctorPatientRelationshipResponse>>> GetDoctorPatients(
        CancellationToken cancellationToken)
        => Ok(await service.GetDoctorPatientsAsync(GetCurrentUserId(), cancellationToken));

    [HttpGet("patient")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<IReadOnlyList<PatientDoctorResponse>>> GetPatientDoctors(
        CancellationToken cancellationToken)
        => Ok(await service.GetPatientDoctorsAsync(GetCurrentUserId(), cancellationToken));

    [HttpDelete("{relationshipId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Deactivate(
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        await service.DeactivateRelationshipAsync(
            GetCurrentUserId(),
            relationshipId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}
