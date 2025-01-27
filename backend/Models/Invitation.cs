using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Invitation
{
    public required string Token { get; set; }
}