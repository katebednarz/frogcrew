using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class CrewedUser
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public string? CrewedPosition { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public CrewedUserDTO convertToCrewedUserDTO() {

        using (var _context = new FrogcrewContext() )
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == UserId);
            return new CrewedUserDTO {
                FullName = $"{user.FirstName} {user.LastName}",
                Position = CrewedPosition,
                ReportTime = ArrivalTime.ToString()
            };
        }
        
        
    }
}
