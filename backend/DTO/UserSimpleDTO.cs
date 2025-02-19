using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class UserSimpleDTO
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
