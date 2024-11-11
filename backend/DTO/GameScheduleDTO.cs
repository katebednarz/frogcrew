using System;
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTO;

public class GameScheduleDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Sport is required.")]
    public string? Sport { get; set; }
    [Required(ErrorMessage = "Season is required.")]
    public string? Season { get; set; }
}
