using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Game
{
    public int Id { get; set; }

    public string? Opponent { get; set; }

    public DateTime? GameDate { get; set; }

    public TimeSpan? GameStart { get; set; }

    public string? Venue { get; set; }

    public bool? IsFinalized { get; set; }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
