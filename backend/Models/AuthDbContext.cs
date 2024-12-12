using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }
}

