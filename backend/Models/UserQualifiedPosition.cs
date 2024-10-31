using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class UserQualifiedPosition
{
    public int UserId { get; set; }

    public string Position { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public UserQualifiedPosition(string positionName)
    {
        Position = positionName;
    }
}
