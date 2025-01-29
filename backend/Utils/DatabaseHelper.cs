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
    
    
}