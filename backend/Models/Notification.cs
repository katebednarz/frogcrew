using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime Date { get; set; }

    public bool IsRead { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
