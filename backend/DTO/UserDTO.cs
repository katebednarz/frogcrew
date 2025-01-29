using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class UserDTO
{
    public int Id { get; set; }
    [Required(ErrorMessage = "First name is required.")]
    public required string? FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required.")]
    public required string? LastName { get; set; }
    [Required(ErrorMessage = "Email is required.")]
    public required string? Email { get; set; }
    [Required(ErrorMessage = "Phone number is required.")]
    public required string? PhoneNumber { get; set; }
    [Required(ErrorMessage = "Role is required.")]
    public required string? Role { get; set; }
    [Required(ErrorMessage = "PositionId is required.")]
    public required List<String>? Position { get; set; }
    
    public string? Password { get; set; }
    
}
