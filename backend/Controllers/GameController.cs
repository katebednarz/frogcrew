using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    public class GameController : Controller
    {
        private readonly FrogcrewContext _context;

        public GameController(FrogcrewContext context)
        {
        _context = context;
        }

        [HttpGet("game/{id}")]
        public async Task<IActionResult> FindGameById(int id) {
            var game = await _context.Games.FindAsync(id);
            if (game == null) {
                return NotFound($"Game with ID {id} not found.");
            }

            return Ok(game);

        }
    }
}
