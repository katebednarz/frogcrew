using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Invitation
{
    public string Token { get; set; }

    public Invitation(){
        Token = Guid.NewGuid().ToString();;
    }
}