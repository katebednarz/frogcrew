using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public record NotificationDTO()
{
    public int Id { get; set; }
    [Required(ErrorMessage = "First name is required")]
    public required string Message { get; set; }
    [Required(ErrorMessage = "First name is required")]
    public required bool IsRead { get; set; }
}