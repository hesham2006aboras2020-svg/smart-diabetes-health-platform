using System.Security.Claims;
using Application.DTOs.Notifications;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> Get(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
        => Ok(await service.GetMyNotificationsAsync(GetUserId(), unreadOnly, cancellationToken));

    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        await service.MarkAsReadAsync(GetUserId(), notificationId, cancellationToken);
        return NoContent();
    }

    [HttpPost("device-token")]
    public async Task<IActionResult> RegisterDeviceToken(
        RegisterDeviceTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.RegisterDeviceTokenAsync(GetUserId(), request, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid user identity.");
    }
}