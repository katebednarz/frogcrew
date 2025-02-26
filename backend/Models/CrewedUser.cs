using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class CrewedUser
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public int PositionId { get; set; }

    public TimeOnly? ArrivalTime { get; set; }
    
    public virtual Position CrewedPositionNavigation { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
