using backend.DTO;
using backend.Models;

namespace backend.Utils;

public class DatabaseHelper
{
    private readonly FrogcrewContext _context;

    public DatabaseHelper(FrogcrewContext context)
    {
        _context = context;
    }
    
    public int? GetPositionIdByName(string positionName) => _context.Positions
        .Where(p => p.PositionName == positionName)
        .Select(p => p.PositionId)
        .FirstOrDefault();
    
    public string? GetPositionNameById(int positionId) => _context.Positions
        .Where(p => p.PositionId == positionId)
        .Select(p => p.PositionName)
        .FirstOrDefault();
    
    public Invitation? GetInvitationByToken(string token) => _context.Invitations
        .FirstOrDefault(i => i.Token == token);
    
    public CrewedUser? GetCrewedUserById(int userId, int gameId) => _context.CrewedUsers
        .FirstOrDefault(u => u.UserId == userId && u.GameId == gameId);
    
    public TradeBoard? GetTradeBoardEntry(TradeRequestInfo request) => _context.TradeBoards
        .FirstOrDefault(p => p.GameId == request.GameId && p.DropperId == request.UserId && p.Position == request.PositionId);
    
    public List<CrewedUser> GetCrewedUsersByGame(int gameId) => _context.CrewedUsers
        .Where(p => p.GameId == gameId).ToList();
    
    public int GetScheduleIdBySportAndSeason(string? sport, string? season) => _context.Schedules
        .Where(s => s.Sport == sport && s.Season == season)
        .Select(s => s.Id)
        .FirstOrDefault();
    
    public int GetGameIdByScheduleIdAndDate(int scheduleId, DateOnly? date) => _context.Games
        .Where(g => g.ScheduleId == scheduleId && g.GameDate == date)
        .Select(g => g.Id)
        .FirstOrDefault();
}