using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // POST /crewMember
        [HttpPost("crewMember")]
        public IActionResult CreateCrewMember([FromBody] CrewMemberRequest request)
        {
            // validate required fields
            if (string.IsNullOrEmpty(request.FirstName) || 
                string.IsNullOrEmpty(request.LastName) ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.PhoneNumber) ||
                string.IsNullOrEmpty(request.Role) ||
                (request.Position == null || request.Position.Count == 0))
            {
                return BadRequest(new ErrorResponse
                {
                    Flag = false,
                    Code = 400,
                    Message = "Provided arguments are invalid, see data for details.",
                    Data = new Dictionary<string, string>
                    {
                        { "firstName", string.IsNullOrEmpty(request.FirstName) ? "first name is required." : null },
                        { "lastName", string.IsNullOrEmpty(request.LastName) ? "last name is required." : null },
                        { "email", string.IsNullOrEmpty(request.Email) ? "email is required." : null },
                        { "phoneNumber", string.IsNullOrEmpty(request.PhoneNumber) ? "phone number is required." : null },
                        { "role", string.IsNullOrEmpty(request.Role) ? "role is required." : null },
                        { "position", request.Position == null || request.Position.Count == 0 ? "position is required." : null }
                    }
                });
            }

            // mocked response for user creation success
            var response = new CrewMemberResponse
            {
                Flag = true,
                Code = 200,
                Message = "User creation success",
                Data = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Role = request.Role,
                    UserQualifiedPositions = request.Position
                        .Select(pos => new UserQualifiedPosition(pos))
                        .ToList()
                }
            };

            return Ok(response);
        }
    }
}