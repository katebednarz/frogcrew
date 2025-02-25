using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("/")]
[ApiController]
public class NotificationController : Controller
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;
    private readonly DtoConverters _converters;
    public NotificationController(FrogcrewContext context)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
        _converters = new DtoConverters(context);
    }

    [HttpGet("/notifications/{userId}")]
    public async Task<IActionResult> GetNotifications(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new Result(false,404, $"Could not find user with id {userId}"));
        
        var notifications = _dbHelper.GetNotificationsByUserId(userId);
        var notificationDtos = new List<NotificationDTO>();
        foreach (var notification in notifications)
        {
            notificationDtos.Add(_converters.NotificationToDto(notification));
        }
        return Ok(new Result(true,200,"Find Success", notificationDtos));
    }

    [HttpPut("/notifications/{notificationId}")]
    public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return NotFound(new Result(false,404, $"Could not find notification with id {notificationId}"));
        
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(new Result(true,200,"Mark Success", _converters.NotificationToDto(notification)));
    }
    
    [HttpDelete("/notifications/{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return NotFound(new Result(false,404, $"Could not find notification with id {notificationId}"));
        
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return Ok(new Result(true,200,"Delete Success"));
    }
}

