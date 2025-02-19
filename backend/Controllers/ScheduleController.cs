using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class ScheduleController : Controller
    {

        private readonly FrogcrewContext _context;
        private readonly DatabaseHelper _dbHelper;
        private readonly DtoConverters _converters;
        public ScheduleController(FrogcrewContext context)
        {
            _context = context;
            _dbHelper = new DatabaseHelper(context);
            _converters = new DtoConverters(context);
        }
        
        /*
            * Adds a game schedule
            * 
            * @param request The game schedule to add
            * @return The result of the operation
        */
        [HttpPost("gameSchedule")]
        public IActionResult CreateGameSchedule([FromBody] GameScheduleDTO request)
        {
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

            var response = new Result(true, 200, "Add Success", newSchedule.ConvertToGameScheduleDTO());
            return Ok(response);
        }

        /*
            * Adds games to a game schedule
            * 
            * @param scheduleId The ID of the schedule to add games to
            * @param games The games to add
            * @return The result of the operation
        */
        [HttpPost("gameSchedule/{scheduleId}/games")]
        public async Task<IActionResult> CreateGameScheduleGames(int scheduleId, [FromBody] List<GameDTO> games) {
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
                    Opponent = game.Opponent,
                    IsFinalized = false
                };
                game.ScheduleId = scheduleId;
                _context.Games.Add(newGame);
                _context.SaveChanges();
                game.GameId = newGame.Id;
            }

            return Ok(new Result(true, 200, "Add Success", games));
        }

        /*
            * Finds a game schedule by ID
            * 
            * @param scheduleId The ID of the schedule
            * @return The result of the operation
        */
        [HttpGet("gameSchedule/{scheduleId}")]
        public async Task<IActionResult> FindScheduleById(int scheduleId) {
            var schedule = await _context.Schedules.FindAsync(scheduleId);

            if (schedule == null) {
                return new ObjectResult(new Result(false, 404, $"Could not find schedule with Id {scheduleId}.")) { StatusCode = 404 };
            }

            var gameScheduleDTO = new GameScheduleDTO {
                Id = schedule.Id,
                Sport = schedule.Sport,
                Season = schedule.Season
            };
            
            return Ok(new Result(true, 200, "Find Success", gameScheduleDTO));
        }

        /*
            * Finds game schedules by season
            * 
            * @param season The season to find schedules for
            * @return The result of the operation
        */
        [HttpGet("gameSchedule/season/{season}")]
        public async Task<IActionResult> FindGameSchedulesBySeason(string season) {
            var schedules = await _context.Schedules.Where(s => s.Season == season).ToListAsync();

            if (schedules == null || schedules.Count == 0) {
                return new ObjectResult(new Result(false, 404, $"Could not find any schedules for season {season}.")) { StatusCode = 404 };
            }
            
            var gameScheduleDTOs = schedules.Select(s => new GameScheduleDTO {
                Id = s.Id,
                Sport = s.Sport,
                Season = s.Season
            }).ToList();

            return Ok(new Result(true, 200, "Find Success", gameScheduleDTOs));
        }

        [HttpPut("gameSchedule/{scheduleId}")]
        public async Task<IActionResult> UpdateGameSchedule([FromBody] GameScheduleDTO request, int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule is null)
                return new ObjectResult(new Result(false, 404, $"Could not find schedule with id {scheduleId}")) { StatusCode = 400 };
        
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(kvp => kvp.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);
        
                return new ObjectResult(errorResponse) { StatusCode = 400 };
            }
            
            var x = _dbHelper.GetScheduleIdBySportAndSeason(request.Sport, request.Season);
            if (x > 0)
                return new ObjectResult(new Result(false, 409, "Schedule already exists")) { StatusCode = 409 };
        
            schedule.Sport = request.Sport;
            schedule.Season = request.Season;
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
            return Ok(new Result(true, 200, "Update Success", _converters.ScheduleToGameScheduleDto(schedule)));
        }
    }
}
