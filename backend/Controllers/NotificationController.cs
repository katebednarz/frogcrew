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
        return Ok();
    }

    [HttpDelete("/notifications/{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        return Ok();
    }
}

