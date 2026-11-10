using Application.DTOs.Patient;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/patient/profile")]
[Authorize(Roles = "Patient")]
public sealed class PatientProfileController(
    IPatientProfileService patientProfileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PatientProfileResponse>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await patientProfileService.GetAsync(GetUserId(), cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<PatientProfileResponse>> Update(
        UpdatePatientProfileRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await patientProfileService.UpdateAsync(
            GetUserId(),
            request,
            cancellationToken));
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("تعذر تحديد هوية المستخدم.");

        return userId;
    }
}