using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class CrewedUserController : Controller
    {

        private readonly FrogcrewContext _context;

        public CrewedUserController(FrogcrewContext context)
        {
        _context = context;
        }

        /*
            * Adds a crewed user
            * 
            * @param request The crewed user to add
            * @return The result of the operation
        */
        [HttpGet("crewMember/{gameId}/{position}")]
        public async Task<IActionResult> FindCrewMemberByGameAndPosition(int gameId, string position) {
            var availableQualifiedUsers = await _context.Users
            .Where(u => u.Availabilities.Any(a => a.GameId == gameId && a.Available)
                     && u.UserQualifiedPositions.Any(qp => qp.Position == position))
            .Select(u => new UserSimpleDTO
            {
                UserId = u.Id,
                FullName = $"{u.FirstName} {u.LastName}"
            })
            .ToListAsync();

            if (availableQualifiedUsers.Count == 0)
            {
                return NotFound(new Result(false, 404, $"No matching crew members available for {position}", null));
            }

            return Ok(new Result(true, 200, "Find Success", availableQualifiedUsers));
        }

    }
}
