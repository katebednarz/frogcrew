namespace backend.DTO;

public record TradeRequestInfo
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public int PositionId { get; set; }
}