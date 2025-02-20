using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ResetPasswordToken
{
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
