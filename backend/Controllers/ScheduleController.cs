using System.Text;
using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class ScheduleController : Controller
    {

        private readonly FrogcrewContext _context;
        public ScheduleController(FrogcrewContext context)
        {
            _context = context;
        }

        // GET: ScheduleController
        [HttpGet]
        public ActionResult Index()
        {
            return Ok("Hello from schedule!");
        }

        // POST /gameSchedule
        [HttpPost("gameSchedule")]
        public async Task<IActionResult> CreateGameSchedule([FromBody] GameScheduleDTO request) { 
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                        .SelectMany(kvp => kvp.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);

                return new ObjectResult(errorResponse) { StatusCode = 400 };
            }

            var newSchedule = new Schedule
            {
                Sport = request.Sport,
                Season = request.Season
            };

            _context.Add(newSchedule);
            _context.SaveChanges();

            var response = new Result(true, 200, "Add Success", newSchedule);
            return Ok(response);
        }

        [HttpPost("gameSchedule/{scheduleId}/games")]
        public async Task<IActionResult> FindScheduleById(int scheduleId, [FromBody] List<GameDTO> games) {
            var gameSchedule = await _context.Schedules.FindAsync(scheduleId);
            if (gameSchedule == null) {
                return new ObjectResult(new Result(false, 404, $"Could not find schedule with ID {scheduleId}.")) { StatusCode = 404 };
            }
            if (games == null || games.Count == 0) {
                return BadRequest(new Result(false, 400, "Games list cannot be null or empty."));
            }

            foreach (var game in games) {
                var newGame = new Game {
                    ScheduleId = scheduleId,
                    Schedule = gameSchedule,
                    GameDate = game.GameDate,
                    Venue = game.Venue,
                    Opponent = game.Opponent
                };
                game.ScheduleId = scheduleId;
                _context.Games.Add(newGame);
                _context.SaveChanges();
                game.GameId = newGame.Id;
            }

            return Ok(new Result(true, 200, "Add Success", games));
        }
    }
}
