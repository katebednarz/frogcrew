using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class User
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int Id { get; set; }

    private string Email { get; set; }
    private string Password { get; set; }  // Assume encrypted
    private int PhoneNumber { get; set; }
    private string FirstName { get; set; }
    private string LastName { get; set; }
    private string Role { get; set; }
    private List<string> QualifiedPositions { get; set; }
    private string PayRate { get; set; }

    public void Register() { /* Implementation */ }
    public void Login() { /* Implementation */ }
    public void Invite() { /* Implementation */ }

}

