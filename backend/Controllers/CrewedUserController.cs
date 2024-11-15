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

        [HttpGet("crewMember/{gameId}/{position}")]
        public async Task<IActionResult> FindCrewMemberByGameAndPosition(int gameId, string position) {
            var availableUsers = await _context.Users
            .Where(u => u.Availabilities
                .Any(a => a.GameId == gameId && a.Open)) // Check user availability for the game
            .Where(u => u.UserQualifiedPositions
                .Any(qp => qp.Position == position)) // Check user qualifications for the position
            .ToListAsync();

            var AvailableQualifiedUsers = new List<UserSimpleDTO>();
            foreach (var user in availableUsers) {
                AvailableQualifiedUsers.Add(user.ConvertToUserSimpleDTO());
            }

            return Ok(new Result(true, 200, "Find Success", AvailableQualifiedUsers));
        }

    }
}
