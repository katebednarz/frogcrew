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
using backend.Utils;
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
  private readonly DatabaseHelper _dbHelper;
    private readonly NotificationsHelper _notificationsHelper;

  public UserController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      FrogcrewContext context,
      IConfiguration configuration,
        NotificationsHelper notificationsHelper)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _context = context;
    _configuration = configuration;
    _dbHelper = new DatabaseHelper(context);
        _notificationsHelper = notificationsHelper;
  }

  /*
   * Adds a crew member
   *
   * @param request The crew member to add
   * @return The result of the operation
   */
  [HttpPost("crewMember")]
  public async Task<IActionResult> CreateCrewMember([FromBody] UserDTO request, String token)
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
      IsActive = true
    };
    if (request.Password == null)
    {
      request.Password = "Password!1";
    }
    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errors = result.Errors.Select(e => e.Description).ToList();
      return BadRequest(new Result(false, 400, "User creation failed", errors));
    }

    var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
    if (!roleResult.Succeeded)
    {
      await _userManager.DeleteAsync(user);
      return new ObjectResult(new Result(false, 400, "Role not found", request.Role));
    }
_context.Users.Attach(user);
    foreach (var pos in request.Position)
    {
      var newPosition = new UserQualifiedPosition
      {
        UserId = user.Id,
        PositionId = (int)_dbHelper.GetPositionIdByName(pos)
      };
      _context.Add(newPosition);
      _context.SaveChanges();
    }

    if (token != "token")
    {
      _context.Invitations.Remove(_dbHelper.GetInvitationByToken(token));
      _context.SaveChanges();
    }
        
        string notificationMessage = NotificationContent.GetNotificationTemplate("UserCreatedNotification", [request.FirstName, request.LastName]);
        _notificationsHelper.SendNotificationToAdmin(notificationMessage);

    return Ok(new Result(true, 200, "Add Success", request));
  }

  // validates invite token
  [HttpGet("invite/{token}")]
  public async Task<IActionResult> ValidateInvitation(string token)
  {
    if (string.IsNullOrEmpty(token))
    {
      return BadRequest(new Result(false, 400, "Token is required", null));
    }

    var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token);
    if (invitation == null)
    {
      return NotFound(new Result(false, 404, "Invitation not valid", null));
    }

    return Ok(new Result(true, 200, "Invitation valid", new { token }));
  }

  /*
   * Invite a crew member
   *
   * @param request The emails to send inviations to
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
  private void SendInviteEmail(string email)
  {
    // generate unique invite token
    var invitation = new Invitation();
    var inviteLink = $"http://localhost:5173/register?token={invitation.Token}";

    _context.AddAsync(invitation);
    _context.SaveChangesAsync();

    // email setup
    var fromAddress = new MailAddress("frog.crew.invitation@gmail.com", "FrogCrew");
    var toAddress = new MailAddress(email);
    const string fromPassword = "icbu ddnf yuhi lssz"; // gmail app key
    const string subject = "Invitation to Join FrogCrew";
    string body = "You have been invited to join our crew! Please click the link below to accept the invitation:\n\n" + inviteLink;

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
    var user = await _userManager.FindByEmailAsync(Email);
    if (user == null)
      return new ObjectResult(new Result(false, 401, "username or password is incorrect")) { StatusCode = 401 };

    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, Password, false);
    if (!signInResult.Succeeded)
      return new ObjectResult(new Result(false, 401, "username or password is incorrect")) { StatusCode = 401 };

    if (user.IsActive != true)
    {
      return new ObjectResult(new Result(false, 404, "User is not active", null));
    }

    var token = GenerateJwtToken(user);

    var authDTO = new AuthDTO
    {
      UserId = user.Id,
      Role = _userManager.GetRolesAsync(user).Result.First(),
      Token = token
    };

    return Ok(new Result(true, 200, "Login successful", authDTO));
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
        var users = _dbHelper.GetNonAdminUser();
        List<UserSimpleDTO> userDTOs = [];
        foreach (var user in users)
        {
            if (user.IsActive)
            {
                var userDto = new UserSimpleDTO
                {
                    UserId = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };

        userDTOs.Add(userDto);
      }
    }

    var response = new Result(true, 200, "Found Users", userDTOs);
    return Ok(response);
  }

  /*
   * Find a crew member by userId
   *
   * @param request The id of the user to find
   * @return The result of the operation
   */
  [HttpGet("crewMember/{userId}")]
  public async Task<IActionResult> FindUserById(int userId)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
    {
      return new ObjectResult(new Result(false, 404, $"User with ID {userId} not found.")) { StatusCode = 404 };
    }

    if (user.IsActive != true)
    {
      return new ObjectResult(new Result(false, 404, "User is not active", null));
    }

    var usersQualifiedPosition = await _context.UserQualifiedPositions
        .Where(u => u.UserId == userId)
        .Select(u => u.Position)
        .ToListAsync(); // Materialize the query

    List<string> Positions = new List<string>();

    var usersQualifiedPositionList = usersQualifiedPosition.ToList();

    foreach (Position position in usersQualifiedPositionList)
    {
      Positions.Add(position.PositionName);
    }


    var foundUserDto = new FoundUserDTO()
    {
      UserId = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      PhoneNumber = user.PhoneNumber,
      Role = _userManager.GetRolesAsync(user).Result.First(),
      Positions = Positions
    };

    return Ok(new Result(true, 200, "Find Success", foundUserDto));
  }

  /*
   * Update a crew member by userId
   *
   * @param request The id of the user to find
   * @return The result of the operation
   */
  [HttpPut("crewMember/{userId}")]
  public async Task<IActionResult> UpdateUserByUserId([FromBody] UserDTO request, int userId)
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

    var user = await _context.Users.FindAsync(userId);
    if (user == null)
    {
      return NotFound(new Result(false, 404, $"Could not find user with ID {userId}."));
    }

    // Update user properties
    user.FirstName = request.FirstName;
    user.LastName = request.LastName;
    user.Email = request.Email;
    user.PhoneNumber = request.PhoneNumber;

    // Update role
    var currentRoles = await _userManager.GetRolesAsync(user);
    await _userManager.RemoveFromRolesAsync(user, currentRoles);
    var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
    if (!roleResult.Succeeded)
    {
      return BadRequest(new Result(false, 400, "Role update failed", roleResult.Errors.Select(e => e.Description).ToList()));
    }

    // Remove old positions
    var oldPositions = _context.UserQualifiedPositions.Where(up => up.UserId == user.Id);
    _context.UserQualifiedPositions.RemoveRange(oldPositions);
    await _context.SaveChangesAsync(); // Ensure old positions are removed before adding new ones

    // Add new positions
    foreach (var posName in request.Position)
    {
      var positionId = (int)_dbHelper.GetPositionIdByName(posName)!;
      if (!(positionId > 0)) // Check if position exists
      {
        return BadRequest(new Result(false, 400, $"Position '{posName}' not found."));
      }

      var newPosition = new UserQualifiedPosition
      {
        UserId = user.Id,
        PositionId = (int)positionId // Extract value safely
      };
      _context.UserQualifiedPositions.Add(newPosition);
    }

    await _context.SaveChangesAsync(); // Save all changes

    // Prepare response
    var updatedUser = new FoundUserDTO
    {
      UserId = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      PhoneNumber = user.PhoneNumber,
      Role = request.Role,
      Positions = request.Position
    };

    return Ok(new Result(true, 200, "Update Success", updatedUser));
  }

  [HttpPut("crewMember/disable/{userId}")]
  public async Task<IActionResult> DisableUser(int userId)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
    {
      return NotFound(new Result(false, 404, $"User with ID {userId} not found."));
    }

    user.IsActive = false;
    await _context.SaveChangesAsync(); // Save all changes
    return Ok(new Result(true, 200, "Disable Success", null));
  }
  /*
   * Reset password for a user
   *
   * @param request The email and new password for the user
   * @return The result of the operation
   */
  [HttpPost("auth/resetPassword")]
  public async Task<IActionResult> ResetPassword([FromBody] PasswordDTO request)
  {
    if (!ModelState.IsValid)
    {
      var errors = ModelState
          .SelectMany(kvp => kvp.Value.Errors)
          .Select(e => e.ErrorMessage)
          .ToList();
      return new ObjectResult(new Result(false, 400, "Provided arguments are invalid, see data for details.", errors)) { StatusCode = 400 };
    }

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
      return NotFound(new Result(false, 404, $"User with email {request.Email} not found."));
    }
    
    var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

    if (!result.Succeeded)
    {
      var errors = result.Errors.Select(e => e.Description).ToList();
      return BadRequest(new Result(false, 400, "Password reset failed", errors));
    }

    return Ok(new Result(true, 200, "Password reset successful", null));
  }

  [HttpPost("auth/forgotPassword/{email}")]
  public async Task<IActionResult> ForgotPassword(String email)
  {
    var user = await _userManager.FindByEmailAsync(email);
    
    // generate unique invite token
    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
    var resetLink = $"ADD LINK TO RESET PASSWORD HERE (token={resetToken})";

    // email setup
    var fromAddress = new MailAddress("frog.crew.invitation@gmail.com", "FrogCrew");
    var toAddress = new MailAddress(email);
    const string fromPassword = "icbu ddnf yuhi lssz"; // gmail app key
    const string subject = "FrogCrew Reset Password";
    string body = "Please click the link below to reset your password for FrogCrew:\n\n" + resetLink;

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
    
    var response = new Result(true, 200, "Password reset email sent successfully", null);
    return Ok(response);
  }
}

