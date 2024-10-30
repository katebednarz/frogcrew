using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? Date { get; set; }

    public virtual User? User { get; set; }
}
