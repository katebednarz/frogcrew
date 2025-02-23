using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public required string PositionName { get; set; }

    public required string PositionLocation { get; set; }

    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

    //public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();
    public ICollection<UserQualifiedPosition> UserQualifiedPositions { get; set; } = new List<UserQualifiedPosition>();
    
    public virtual ICollection<TradeBoard> TradeBoards { get; set; } = new List<TradeBoard>();
}
