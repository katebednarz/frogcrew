using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class UserQualifiedPosition
{
    public int UserId { get; set; }

    public int PositionId { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    
    public virtual Position Position { get; set; } = null!;
}
