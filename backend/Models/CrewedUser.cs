using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class CrewedUser
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public required string CrewedPosition { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public CrewedUserDTO ConvertToCrewedUserDTO(FrogcrewContext _context)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == UserId);
        return new CrewedUserDTO
        {
            UserId = UserId,
            FullName = $"{user?.FirstName} {user?.LastName}",
            Position = CrewedPosition,
            ReportTime = ArrivalTime.ToString()
        };
    }
}
