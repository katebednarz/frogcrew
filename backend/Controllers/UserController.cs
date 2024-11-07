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
    }
}