using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponseDto>>> GetMine(bool unreadOnly = false, CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid authenticated user." });
        return Ok(await notifications.GetForUserAsync(userId, unreadOnly, ct));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountDto>> GetUnreadCount(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid authenticated user." });
        return Ok(new UnreadNotificationCountDto(await notifications.GetUnreadCountAsync(userId, ct)));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<NotificationResponseDto>> MarkAsRead(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid authenticated user." });
        var result = await notifications.MarkAsReadAsync(id, userId, ct);
        return result is null ? NotFound(new { message = "Notification not found." }) : Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "Invalid authenticated user." });
        await notifications.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) && userId > 0;
}
