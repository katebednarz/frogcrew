using System;
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTO;

public class GameScheduleRequest
{
    [Required(ErrorMessage = "Sport is required.")]
    public string? Sport { get; set; }
    [Required(ErrorMessage = "Season is required.")]
    public string? Season { get; set; }
    [Required(ErrorMessage = "List of games is required.")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
