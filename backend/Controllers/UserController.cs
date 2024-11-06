using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

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