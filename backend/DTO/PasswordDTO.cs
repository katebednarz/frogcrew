using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public record PasswordDTO()
{
    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; set; }
    
    [Required(ErrorMessage = "NewPassword is required")]
    public required string NewPassword { get; set; }
}