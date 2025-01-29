using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; } = null!;

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    //public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<UserQualifiedPosition> UserQualifiedPositions { get; set; } = new List<UserQualifiedPosition>();
}
