using System.Security.Claims;
using Application.DTOs.DoctorNotes;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/doctor-notes")]
public sealed class DoctorNotesController(IDoctorNoteService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<DoctorNoteResponse>> Create(
        [FromBody] CreateDoctorNoteRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.CreateAsync(GetCurrentUserId(), request, cancellationToken));

    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<IReadOnlyList<DoctorNoteResponse>>> GetForPatient(
        Guid patientId,
        CancellationToken cancellationToken)
        => Ok(await service.GetForPatientAsync(GetCurrentUserId(), patientId, cancellationToken));

    [HttpGet("my")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<IReadOnlyList<DoctorNoteResponse>>> GetMyNotes(
        CancellationToken cancellationToken)
        => Ok(await service.GetMyNotesAsync(GetCurrentUserId(), cancellationToken));

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}
