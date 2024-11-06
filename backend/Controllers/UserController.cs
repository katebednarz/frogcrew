using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly FrogcrewContext _context;
        public UserController(FrogcrewContext context)
        {
            _context = context;
        }

        // POST /crewMember
        [HttpPost("crewMember")]
        public async Task<IActionResult> CreateCrewMember([FromBody] CrewMemberRequest request) {   

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

            var response = new Result(true, 200, "Add Success", newUser);
            return Ok(response);
        }
        
    }
}