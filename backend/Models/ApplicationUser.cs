using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(50)]
    public required string? FirstName { get; set; }
    [MaxLength(50)]
    public required string? LastName { get; set; }
    [MaxLength(25)]
    public string? PayRate { get; set; }
    
}

