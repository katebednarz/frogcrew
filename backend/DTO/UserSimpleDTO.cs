using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class UserSimpleDTO
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    
}
