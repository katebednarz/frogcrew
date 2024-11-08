using System;
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTO;

public class GameCreationDTO
{
    public int ScheduleId { get; set; }
    [Required(ErrorMessage = "Game date is required.")]
    public DateOnly? GameDate { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    public string? Venue { get; set; }

    [Required(ErrorMessage = "Opponent is required.")]
    public string? Opponent { get; set; }
}
