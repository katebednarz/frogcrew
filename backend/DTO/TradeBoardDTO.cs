namespace backend.DTO;

public class TradeBoardDTO
{
    public int TradeId { get; set; }
    public  int DropperId { get; set; }
    public  int GameId { get; set; }
    public required string Position { get; set; }
    public required string Status { get; set; }
    public int? ReceiverId { get; set; }
}