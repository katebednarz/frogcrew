using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Template
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public virtual ICollection<TemplatePosition> TemplatePositions { get; set; } = new List<TemplatePosition>();
}
