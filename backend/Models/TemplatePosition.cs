using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TemplatePosition
{
    public int TemplateId { get; set; }

    public int PositionId { get; set; }

    public decimal HoursBeforeGameTime { get; set; }

    public virtual Position Position { get; set; } = null!;

    public virtual Template Template { get; set; } = null!;
}
