using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class CrewedUser
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public int PositionId { get; set; }

    public TimeOnly ArrivalTime { get; set; } = new TimeOnly(0, 0, 0);
    
    public virtual Position CrewedPositionNavigation { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;

    public CrewedUserDTO ConvertToCrewedUserDTO(FrogcrewContext _context)
    {
        var positionName = _context.Positions.FirstOrDefault(p => p.PositionId == PositionId)?.PositionName;
        return new CrewedUserDTO
        {
            UserId = this.UserId,
            GameId = this.GameId,
            Position = positionName!,
            ArrivalTime = this.ArrivalTime.ToString()
        };
    }
}
