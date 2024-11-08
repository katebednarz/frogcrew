using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class Game
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public string? Opponent { get; set; }

    public DateOnly? GameDate { get; set; }

    public TimeSpan? GameStart { get; set; }

    public string? Venue { get; set; }

    public bool IsFinalized { get; set; }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    public virtual Schedule? Schedule { get; set; }
    
    public GameDetailedDTO convertToGameDetailedDTO() {
        return new GameDetailedDTO {
            GameId = Id,
            ScheduleId = ScheduleId,
            GameDate = GameDate,
            Venue = Venue,
            Opponent = Opponent,
            Finalized = IsFinalized,
            CrewedMembers = CrewedUsersToDTOList()
        };
    }

    private List<CrewedUserDTO> CrewedUsersToDTOList() {
        Console.WriteLine("debug");
        var DTOList = new List<CrewedUserDTO>();

        foreach (var CrewedUser in CrewedUsers)
        {
            DTOList.Add(CrewedUser.convertToCrewedUserDTO());
        }

        return DTOList;
    }
    
}
