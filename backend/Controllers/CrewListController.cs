using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class CrewListController : Controller
    {
        
        private readonly FrogcrewContext _context;

        public CrewListController(FrogcrewContext context)
        {
        _context = context;
        }

        [HttpGet("crewList/{gameId}")]
        public async Task<IActionResult> FindCrewListById(int gameId) {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }

            return Ok(new Result(true, 200, "Find Success", game.ConvertToCrewListDTO()));
        }
    }
}
