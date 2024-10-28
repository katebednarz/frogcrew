using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Game
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int Id { get; set; }

    private Schedule Schedule { get; set; }
    private DateTime GameStart { get; set; }
    private string Venue { get; set; }
    private bool IsFinalized { get; set; }
    private List<string> OpenPositions { get; set; }
    private List<User> CrewMembers { get; set; }

}
