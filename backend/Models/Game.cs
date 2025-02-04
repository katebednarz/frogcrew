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

    public TimeOnly? GameStart { get; set; }

    public string? Venue { get; set; }

    public bool IsFinalized { get; set; }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    public virtual Schedule? Schedule { get; set; }
    
    public virtual ICollection<TradeBoard> GamesOnTradeBoard { get; set; } = new List<TradeBoard>();

    public GameDTO ConvertToGameDTO() {
        return new GameDTO {
            GameId = Id,
            ScheduleId = ScheduleId,
            GameDate = GameDate,
            Venue = Venue,
            Opponent = Opponent,
            IsFinalized = IsFinalized
        };
    }
    
    public CrewListDTO ConvertToCrewListDTO(FrogcrewContext _context) {
        return new CrewListDTO {
            GameId = Id,
            GameStart = GameStart,
            GameDate = GameDate,
            Venue = Venue,
            Opponent = Opponent,
            CrewedMembers = CrewedUsersToDTOList(_context)
        };
    }
    
    private List<CrewedUserDTO> CrewedUsersToDTOList(FrogcrewContext _context) {
        var CrewedUserDTOList = new List<CrewedUserDTO>();
        var CrewedUserList = _context.CrewedUsers.Where(c => c.GameId == Id).ToList(); 
        foreach (var CrewedUser in CrewedUserList)
        {
            CrewedUserDTOList.Add(CrewedUser.ConvertToCrewedUserDTO(_context));
        }
    
        return CrewedUserDTOList;
    }
}
