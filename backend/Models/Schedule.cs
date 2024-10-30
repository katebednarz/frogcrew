using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Schedule
{
    public int Id { get; set; }

    public string? Sport { get; set; }

    public string? Season { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
