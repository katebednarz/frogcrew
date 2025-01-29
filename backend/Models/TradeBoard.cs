using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TradeBoard
{
    public int DropperId { get; set; }

    public int GameId { get; set; }

    public int Position { get; set; }

    public string Status { get; set; } = null!;

    public int? ReceiverId { get; set; }
}
