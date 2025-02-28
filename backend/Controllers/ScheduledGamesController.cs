using backend.Models;
using Microsoft.AspNetCore.Mvc;
using backend.Utils;
using backend.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Route("/scheduledGames/")]
[ApiController]
public class ScheduledGamesController : Controller
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;
    private readonly DtoConverters _converters;
    private readonly NotificationsHelper _notificationsHelper;

    public ScheduledGamesController(FrogcrewContext context, NotificationsHelper notificationsHelper)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
        _converters = new DtoConverters(context);
        _notificationsHelper = notificationsHelper;
    }

    [HttpPost("drop")]
    public async Task<IActionResult> DropShift([FromBody] TradeRequestInfo request)
    {
        if (_dbHelper.GetTradeBoardEntry(request) is not null)
        {
            return Ok(new Result(true, 200, "Shift Already Dropped"));
        }
        var game = await _context.Games.Include(g => g.Schedule).FirstOrDefaultAsync(g => g.Id == request.GameId);
        if (game is null) return new ObjectResult(new Result(false, 404, $"Could not find game with id {request.GameId}")) { StatusCode = 404 };
        var dropper = await _context.Users.FindAsync(request.UserId);
        if (dropper is null) return new ObjectResult(new Result(false, 404, $"Could not find user with id {request.UserId}")) {StatusCode = 404};

        var crewedUser = await _context.CrewedUsers.FindAsync(request.UserId, request.GameId, request.PositionId);
        if (crewedUser is null) return new ObjectResult(new Result(false, 404, $"Could not find shift")) { StatusCode = 404 };
        
        TradeBoard tb = new TradeBoard
        {
            DropperId = request.UserId,
            GameId = request.GameId,
            Position = crewedUser.PositionId,
            Status = "AVAILABLE",
        };
        
        _context.TradeBoards.Add(tb);
        await _context.SaveChangesAsync();
        
        var positionId = tb.Position;
        var positionName = _dbHelper.GetPositionNameById(positionId);
        
        string notificationMessage = NotificationContent.GetNotificationTemplate("DroppedShiftNotification", [dropper.FirstName, dropper.LastName, positionName, game.Schedule.Sport, game.GameDate]);
        _notificationsHelper.SendNotificationToAdmin(notificationMessage);
        
        var qualifiedUsers = await _context.Users
            .Where(u => u.UserQualifiedPositions.Any(qp => qp.PositionId == positionId))
            .ToListAsync();
        
        foreach (var user in qualifiedUsers)
        {
            _notificationsHelper.SendNotificationToCrewMember(user.Id, notificationMessage);
        }
        
        return Ok(new Result(true, 200, "Drop Success", _converters.TradeBoardToDto(tb)));
    }

    [HttpPut("pickup/{tradeId}/{userId}")]
    public async Task<IActionResult> PickupShift(int tradeId, int userId)
    {
        var trade = await _context.TradeBoards.FindAsync(tradeId);
        if (trade is null) return new ObjectResult(new Result(false, 404, $"Could not find trade with id {tradeId}")) { StatusCode = 404 };
        var pickerUpper = await _context.Users.FindAsync(userId);
        if (pickerUpper is null) return new ObjectResult(new Result(false, 404, $"Could not find user with id {userId}")) {StatusCode = 404};
        var usersQualifiedPosition = await _context.UserQualifiedPositions
            .Where(u => u.UserId == userId)
            .Select(u => u.PositionId)
            .ToListAsync(); // Materialize the query
        if (!usersQualifiedPosition.Contains(trade.Position)) 
            return new ObjectResult(new Result(false, 400, "User is not qualified for this position"));
        
        var dropper = await _context.Users.FindAsync(trade.DropperId);
        var game = await _context.Games.Include(g => g.Schedule).FirstOrDefaultAsync(g => g.Id == trade.GameId);
        
        trade.ReceiverId = userId;
        trade.Status = "AWAITING APPROVAL";
        string notificationMessage = NotificationContent.GetNotificationTemplate("PickedUpShiftNotification", [pickerUpper.FirstName, pickerUpper.LastName, dropper.FirstName, dropper.LastName, game.Schedule.Sport, game.GameDate]);
        _notificationsHelper.SendNotificationToAdmin(notificationMessage);
        await _context.SaveChangesAsync();
        return Ok(new Result(true, 200, "Request Success", _converters.TradeBoardToDto(trade)));
    }

    [HttpPut("approve/{tradeId}")]
    public async Task<IActionResult> ApproveShift(int tradeId)
    {
        var trade = await _context.TradeBoards.FindAsync(tradeId);
        if (trade is null) return new ObjectResult(new Result(false, 404, $"Could not find trade with id {tradeId}")) { StatusCode = 404 };
        
        var crewedUser = await _context.CrewedUsers.FindAsync(trade.DropperId, trade.GameId, trade.Position);
        
        trade.Status = "APPROVED";
        
        var newCrewedUser = new CrewedUser
        {
            UserId = (int)trade.ReceiverId!,
            GameId = crewedUser!.GameId,
            PositionId = crewedUser.PositionId,
            ArrivalTime = crewedUser.ArrivalTime,
        };
        
        _context.CrewedUsers.Remove(crewedUser!);
        await _context.CrewedUsers.AddAsync(newCrewedUser);
        await _context.SaveChangesAsync();
        
        var game = await _context.Games.Include(g => g.Schedule).FirstOrDefaultAsync(g => g.Id == trade.GameId);
        
        string notificationMessage = NotificationContent.GetNotificationTemplate("ApprovedShiftPickUpNotification", [game.Schedule.Sport, game.GameDate]);
        _notificationsHelper.SendNotificationToCrewMember(trade.DropperId, notificationMessage);    
        
        return Ok(new Result(true, 200, "Approval Success", _converters.TradeBoardToDto(trade)));
    }

    [HttpGet("tradeboard")]
    public async Task<IActionResult> GetTradeBoard()
    {
        List<TradeBoard> entries = await _context.TradeBoards.Where(u => u.Status == "AVAILABLE").ToListAsync();
        List<TradeBoardDTO> dtos = new List<TradeBoardDTO>();
        foreach (var entry in entries)
        {
            dtos.Add(_converters.TradeBoardToDto(entry));
        }

        return Ok(new Result(true, 200, "Find Success", dtos));
    }
}
