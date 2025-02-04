using backend.Models;
using Microsoft.AspNetCore.Mvc;
using backend.Utils;
using backend.DTO;

namespace backend.Controllers;

[Route("/scheduledGames/")]
[ApiController]
public class ScheduledGamesController : Controller
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;
    private readonly DtoConverters _converters;

    public ScheduledGamesController(FrogcrewContext context)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
        _converters = new DtoConverters(context);
    }

    [HttpPost("drop")]
    public async Task<IActionResult> Drop([FromBody] TradeRequestInfo request)
    {
        var game = await _context.Games.FindAsync(request.GameId);
        if (game is null) return new ObjectResult(new Result(false, 404, $"Could not find game with id {request.GameId}")) { StatusCode = 404 };
        var user = await _context.Users.FindAsync(request.UserId);
        if (user is null) return new ObjectResult(new Result(false, 404, $"Could not find user with id {request.UserId}")) {StatusCode = 404};

        var crewedUser = await _context.CrewedUsers.FindAsync(request.UserId, request.GameId, request.PositionId);
        if (crewedUser is null) return new ObjectResult(new Result(false, 404, $"Could not find shift")) { StatusCode = 404 };
        
        TradeBoard tb = new TradeBoard
        {
            DropperId = request.UserId,
            GameId = request.GameId,
            Position = crewedUser.PositionId,
            Status = "AVAILABLE",
        };

        if (_dbHelper.GetTradeBoardEntry(request) is null)
        {
            _context.TradeBoards.Add(tb);
            await _context.SaveChangesAsync();
        }
            
        return Ok(new Result(true, 200, "Drop Success", _converters.TradeBoardToDto(tb)));
    }

    [HttpPut("pickup/{tradeId}/{userId}")]
    public async Task<IActionResult> Pickup(int tradeId, int userId)
    {
        var trade = await _context.TradeBoards.FindAsync(tradeId);
        if (trade is null) return new ObjectResult(new Result(false, 404, $"Could not find trade with id {tradeId}")) { StatusCode = 404 };
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return new ObjectResult(new Result(false, 404, $"Could not find game with id {userId}")) {StatusCode = 404};
        if (!(user.UserQualifiedPositions.Any(x => x.PositionId == trade.Position)))
            return new ObjectResult(new Result(false, 400, "User is not qualified for this position"));
        
        trade.ReceiverId = userId;
        trade.Status = "AWAITING APPROVAL";
        await _context.SaveChangesAsync();
        return Ok(new Result(true, 200, "Request Success", _converters.TradeBoardToDto(trade)));
    }

    [HttpPut("approve/{tradeId}")]
    public async Task<IActionResult> Approve(int tradeId)
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
        
        return Ok(new Result(true, 200, "Approval Success", _converters.TradeBoardToDto(trade)));
    }
}