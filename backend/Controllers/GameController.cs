using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Tsp;
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

        [HttpGet("gameSchedule/game/{gameId}")]
        public async Task<IActionResult> FindGameById(int gameId) {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }

            return Ok(new Result(true, 200, "Find Success", game));
        }
    }
}
