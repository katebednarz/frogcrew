using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class CrewedUser
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public string? CrewedPosition { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
