using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using backend.DTO;
using backend.Models;


namespace backend.Controllers;

[Route("/")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FrogcrewContext _context;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        FrogcrewContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserDTO request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(kvp => kvp.Value.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);
        
            return new ObjectResult(errorResponse) { StatusCode = 400 };
        }
        
        var user = new ApplicationUser
        {
            UserName = request.Email, 
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, "Password!1");
        
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            return new ObjectResult(new Result(false, 400, "Role not found", request.Role));
        
        if (!result.Succeeded)
            return new BadRequestObjectResult(result.Errors);
        
        // foreach (var pos in request.Position)
        // {
        //     var newPosition = new UserQualifiedPosition
        //     {
        //         UserId = user.Id,
        //         Position = pos
        //     };
        //     _context.Add(newPosition);
        //     _context.SaveChanges();
        // }
        
        return new ObjectResult(new Result(true, 200, "Resistered successfully.", request));
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(String Email, String Password)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(kvp => kvp.Value.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);
        
            return new ObjectResult(errorResponse) { StatusCode = 400 };
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
            return new ObjectResult(new Result(false, 404, "Invalid credentials"));
        
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, Password, false);
        if (!signInResult.Succeeded)
            return new ObjectResult(new Result(false, 404, "Invalid credentials"));
        
        var token = GenerateJwtToken(user);
        
        // var AuthDTO = new AuthDTO
        // {
        //     UserId = user.Id,
        //     Role = user.Role ?? string.Empty,
        //     Token = token
        // };
        
        return Ok(new Result(true, 200, "Login successful", token));
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}