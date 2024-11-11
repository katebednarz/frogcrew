using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class CrewListDTO
{
    public int GameId { get; set; }
    public TimeSpan? GameStart { get; set; }
    public DateOnly? GameDate { get; set; }
    public string? Venue { get; set; }
    public string? Opponent { get; set; }
    public List<CrewedUserDTO> CrewedMembers { get; set; }
     
}