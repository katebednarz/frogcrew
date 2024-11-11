using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class AvailabilityDTO
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    [Required(ErrorMessage = "Availability is required.")]
    public bool Open { get; set; }
    public string? Comment { get; set; }
}
