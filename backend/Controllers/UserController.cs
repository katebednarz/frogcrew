using System.Net;
using System.Net.Mail;
using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Route("/")]
[ApiController]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FrogcrewContext _context;
    private readonly IConfiguration _configuration;

    public UserController(
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

    /*
     * Adds a crew member
     *
     * @param request The crew member to add
     * @return The result of the operation
     */
    [HttpPost("crewMember")]
    public async Task<IActionResult> CreateCrewMember([FromBody] UserDTO request)
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
        
        foreach (var pos in request.Position)
        {
            var newPosition = new UserQualifiedPosition
            {
                UserId = user.Id,
                Position = pos
            };
            _context.Add(newPosition);
            _context.SaveChanges();
        }
        
        return new ObjectResult(new Result(true, 200, "Add Success", request));
    }

    /*
     * Adds a crew member
     *
     * @param request The crew member to add
     * @return The result of the operation
     */
    [HttpPost("invite")]
    public async Task<IActionResult> InviteCrewMember([FromBody] EmailDTO request)
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

        foreach (var email in request.Emails) SendInviteEmail(email);

        var response = new Result(true, 200, "Invite success", request.Emails);
        return Ok(response);
    }

    /*
     * Sends an email to the user with the invite link
     *
     * @param email The email to send the invite to
     */
    private static void SendInviteEmail(string email)
    {
        // email setup
        var fromAddress = new MailAddress("frog.crew.invitation@gmail.com", "FrogCrew");
        var toAddress = new MailAddress(email);
        const string fromPassword = "icbu ddnf yuhi lssz"; // gmail app key
        const string subject = "Invitation to Join FrogCrew";
        const string body =
            "You have been invited to join our crew! Please click the link below to accept the invitation:\n\nhttp://localhost:5173/register";

        // configure SMTP client
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587, // SMTP port
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using (var message = new MailMessage(fromAddress, toAddress)
               {
                   Subject = subject,
                   Body = body
               })
        {
            smtp.Send(message);
        }
    }

    /*
     * Logs in a user
     *
     * @return The result of the operation
     */
    [HttpPost("auth/login")]
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
    
    /*
        * Finds a crew member by ID
        * 
        * @param userId The ID of the crew member
        * @return The result of the operation
    */

    //need to change to 'crewMember', will need to update frontend
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        
        List<UserSimpleDTO> userDTOs = [];
        foreach (var user in users)
        {
           var userDto = new UserSimpleDTO {
                UserId = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            
            userDTOs.Add(userDto);
        }
        
        var response = new Result(true, 200, "Found Users", userDTOs);
        return Ok(response);
    }
}

