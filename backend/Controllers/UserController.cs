using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{
  [Route("/")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly FrogcrewContext _context;
        private readonly string _jwtSecret = "kate-has-badbarz!--kate-has-badbarz!";
        public UserController(FrogcrewContext context)
        {
        _context = context;
        }

        // POST /crewMember
        [HttpPost("crewMember")]
        public Task<IActionResult> CreateCrewMember([FromBody] UserDTO request) {   

            if (!ModelState.IsValid) {
                var errors = ModelState
                        .SelectMany(kvp => kvp.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);

                return Task.FromResult<IActionResult>(new ObjectResult(errorResponse) { StatusCode = 400 });
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

                foreach( var pos in request.Position) {
                    var newPosition = new UserQualifiedPosition {
                        UserId = newUser.Id,
                        Position = pos
                    };
                    _context.Add(newPosition);
                    _context.SaveChanges();
                }

                
                var response = new Result(true, 200, "Add Success", newUser.ConvertToUserDTO());
                return Task.FromResult<IActionResult>(Ok(response));
        }
























































        [HttpPost("login")]
        public IActionResult Login()
        {
            // Check if Authorization header is present
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized("Missing Authorization Header");

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic "))
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

            return Ok(new Result(true, 200, "Login successful", token));
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

        
    }
}