using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class GameDetailedDTO
{
    public int GameId { get; set; }
    public int ScheduleId { get; set; }
    public DateOnly? GameDate { get; set; }
    public string? Venue { get; set; }
    public string? Opponent { get; set; }
    public Boolean Finalized { get; set; }
    public List<CrewedUserDTO> CrewedMembers { get; set; }
     
}