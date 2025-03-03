using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("/report/")]
public class ReportController
{
    private readonly FrogcrewContext _context;
    private readonly NotificationsHelper _notificationsHelper;
    private readonly DatabaseHelper _dbHelper;

    public ReportController(FrogcrewContext context, NotificationsHelper notificationsHelper, DatabaseHelper dbHelper)
    {
        _context = context;
        _notificationsHelper = notificationsHelper;
        _dbHelper = dbHelper;
    }

    [HttpGet("crewMember/{userId}")]
    public async Task<IActionResult> GetUserReport(int userId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("financial/{gameId}")]
    public async Task<IActionResult> GetFinancialReport(int gameId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("position/{positionId}")]
    public async Task<IActionResult> GetPositionReport(int positionId)
    {
        throw new NotImplementedException();
    }
    
}