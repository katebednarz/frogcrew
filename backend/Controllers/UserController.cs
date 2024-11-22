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

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class UserController : Controller
    {

        private readonly FrogcrewContext _context;
        private readonly string _jwtSecret = "kate-has-badbarz!--kate-has-badbarz!";
        private const string BasicAuthScheme = "Basic";
        public UserController(FrogcrewContext context)
        {
            _context = context;
        }

        // POST /crewMember
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

            var newUser = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                Password = PasswordHasher.HashPassword("password")
            };


            _context.Add(newUser);
            _context.SaveChanges();

            foreach (var pos in request.Position)
            {
                var newPosition = new UserQualifiedPosition
                {
                    UserId = newUser.Id,
                    Position = pos
                };
                _context.Add(newPosition);
                _context.SaveChanges();
            }


            var response = new Result(true, 200, "Add Success", newUser.ConvertToUserDTO());
            return Ok(response);
        }

        // POST /invite
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

            foreach (var email in request.Emails)
            {
                SendInviteEmail(email);
            }

            var response = new Result(true, 200, "Invite success", request.Emails);
            return Ok(response);
        }

        private void SendInviteEmail(string email)
        {
            // email setup
            var fromAddress = new MailAddress("frog.crew.invitation@gmail.com", "FrogCrew");
            var toAddress = new MailAddress(email);
            const string fromPassword = "icbu ddnf yuhi lssz"; // gmail app key
            const string subject = "Invitation to Join FrogCrew";
            const string body = "You have been invited to join our crew! Please click the link below to accept the invitation:\n\nhttp://localhost:5173/register";

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

        [HttpPost("auth/login")]
        public IActionResult Login()
        {
            // Check if Authorization header is present
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized("Missing Authorization Header");

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith(BasicAuthScheme + " "))
                return Unauthorized("Invalid Authorization Header");

            // Decode the Base64 encoded credentials
            var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
            var credentials = decodedUsernamePassword.Split(':', 2);
            if (credentials.Length != 2)
                return Unauthorized("Invalid Authorization Header");

            var username = credentials[0];
            var password = credentials[1];

            // Validate user credentials
            var user = _context.Users.FirstOrDefault(u => u.Email == username);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password))
            {
                return Unauthorized("Invalid email or password");
            }


            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);

            var token = GenerateJwtToken(user);

            var AuthDTO = new AuthDTO
            {
                UserId = user.Id,
                Role = user.Role,
                Token = token
            };

            return Ok(new Result(true, 200, "Login successful", AuthDTO));
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

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
}
