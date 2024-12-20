using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class EmailDTO
{
    [Required(ErrorMessage = "An email must be provided.")]
    public required List<String>? Emails { get; set; }    
}