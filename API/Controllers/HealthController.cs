using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController(DiabetesDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "DiabetesPlatform API"
    });

    [HttpGet("live")]
    public IActionResult Live() => Ok(new
    {
        status = "ok",
        service = "DiabetesPlatform API"
    });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseAvailable = await db.Database.CanConnectAsync(cancellationToken);

            if (!databaseAvailable)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "not_ready",
                    database = "unavailable"
                });
            }

            return Ok(new
            {
                status = "ready",
                database = "available"
            });
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "not_ready",
                database = "unavailable"
            });
        }
    }
}