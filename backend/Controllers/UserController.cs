using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("/crewMember")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly FrogcrewContext _context;
        public UserController(FrogcrewContext context)
        {
            _context = context;
        }

        // POST /crewMember
        [HttpPost("")]
        public async Task<IActionResult> CreateCrewMember([FromBody] UserDTO request) {   

            if (!ModelState.IsValid) {
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
                    Password = "password"
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
                return Ok(response);
        }
        
    }
}