using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Game
{
    public int Id { get; set; }

    public int? ScheduleId { get; set; }

    public string? Opponent { get; set; }

    public DateOnly? GameDate { get; set; }

    public TimeOnly? GameStart { get; set; }

    public string? Venue { get; set; }

    public bool? IsFinalized { get; set; }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    public virtual Schedule? Schedule { get; set; }
}
