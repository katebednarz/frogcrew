using backend.DTO;
using backend.Models;

namespace backend.Utils;

public class DtoConverters
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;

    public DtoConverters(FrogcrewContext context)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
    }
    
    public TradeBoardDTO TradeBoardToDto(TradeBoard tradeBoard)
    {
        return new TradeBoardDTO
        {
            TradeId = tradeBoard.TradeId,
            DropperId = tradeBoard.DropperId,
            GameId = tradeBoard.GameId,
            Position = _dbHelper.GetPositionNameById(tradeBoard.Position)!,
            Status = tradeBoard.Status,
            ReceiverId = tradeBoard.ReceiverId
        };

    }
    
}