using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class CrewedUser
{

    private User User { get; set; }
    private Game Game { get; set; }
    private string CrewedPosition { get; set; }
    private DateTime ArrivalTime { get; set; }

}
