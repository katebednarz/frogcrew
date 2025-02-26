using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class CrewScheduleController : Controller
    {
        private readonly FrogcrewContext _context;

        public CrewScheduleController(FrogcrewContext context)
        {
            _context = context;
        }

        [HttpGet("{gameId:int}")]
        public async Task<IActionResult> Get(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return NotFound(new Result(false, 404, $"Error: No Game with ID: {gameId} exists.", null!));

            var crewScheduleDto = new CrewScheduleDTO
            {
                gameId = gameId,
                changes = await _context.CrewedUsers
                    .Where(cu => cu.GameId == gameId)
                    .Select(cu => new ChangesDTO()
                    {
                        Action = "GET",
                        Id = cu.UserId,
                        FullName = _context.Users
                            .Where(u => u.Id == cu.UserId)
                            .Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
                        Position = _context.Positions
                            .Where(p => p.PositionId == cu.PositionId)
                            .Select(p => p.PositionName).FirstOrDefault()
                    }).ToListAsync()
            };

            return Ok(new Result(true, 200, "Find Success", crewScheduleDto));
        }
    }
}