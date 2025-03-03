using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class Schedule
{
    public int Id { get; set; }

    public string? Sport { get; set; }

    public string? Season { get; set; }
    
    public bool? IsPublished { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public GameScheduleDTO ConvertToGameScheduleDTO() {
        return new GameScheduleDTO {
            Id = Id,
            Sport = Sport,
            Season = Season
        };
    }
}
