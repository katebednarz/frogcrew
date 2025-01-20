using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base() { }
    
    public ApplicationRole(string name) : base(name) { }
    
}