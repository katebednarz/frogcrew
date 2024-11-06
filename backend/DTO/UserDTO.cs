using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class UserDTO
{
    public int Id { get; set; }
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone number is required.")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; }
    [Required(ErrorMessage = "Position is required.")]
    public List<String> Position { get; set; }

    
}
