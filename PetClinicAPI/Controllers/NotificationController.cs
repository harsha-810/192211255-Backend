using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userIdStr = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { error = "Invalid or missing User ID in token." });
        
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Date)
            .ToListAsync();
            
        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var notification = await _context.Notifications.FindAsync(id);
        
        if (notification == null || notification.UserId != userId)
            return NotFound("Notification not found.");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Marked as read." });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearAll()
    {
        var userIdStr = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { error = "User ID not found in token." });

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        if (notifications.Any())
        {
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "All notifications cleared successfully." });
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userIdStr = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { error = "User ID not found in token." });

        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
            return NotFound(new { error = $"Notification {id} not found." });

        if (notification.UserId != userId)
            return Forbid("Permission denied.");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Notification deleted." });
    }
}
