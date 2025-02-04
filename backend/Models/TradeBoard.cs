using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TradeBoard
{
    public int TradeId { get; set; }

    public int DropperId { get; set; }

    public int GameId { get; set; }

    public int Position { get; set; }

    public string Status { get; set; } = null!;

    public int? ReceiverId { get; set; }

    public virtual ApplicationUser Dropper { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;

    public virtual Position PositionNavigation { get; set; } = null!;

    public virtual ApplicationUser? Receiver { get; set; }
}
