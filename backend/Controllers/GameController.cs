using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Tsp;
using System.Threading.Tasks;
using backend.Utils;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class GameController : Controller
    {
        private readonly FrogcrewContext _context;
        private readonly DatabaseHelper _dbHelper;
        private readonly DtoConverters _converters;

        public GameController(FrogcrewContext context)
        {
            _context = context;
            _dbHelper = new DatabaseHelper(context);
            _converters = new DtoConverters(context);
        }

        /*
            * Find a game by gameId
            * 
            * @param request The id of the game to find
            * @return The result of the operation
        */         
        [HttpGet("gameSchedule/game/{gameId}")] [Authorize]
        public async Task<IActionResult> FindGameById(int gameId) {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }
            return Ok(new Result(true, 200, "Find Success", game.ConvertToGameDTO()));
        }

        /*
            * Find games by scheduleId
            * 
            * @param request The id of the schedule to find games for
            * @return The result of the operation
        */
        [HttpGet("gameSchedule/{scheduleId}/games")]
        public async Task<IActionResult> FindGamesByScheduleId(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);

            if (schedule == null)
            {
                return new ObjectResult(new Result(false, 404, $"Could not find schedule with Id {scheduleId}.")) { StatusCode = 404 };
            }

            // Retrieve games for the schedule
            var games = await _context.Games.Where(g => g.ScheduleId == scheduleId).ToListAsync();

            if (!games.Any())
            {
                return new ObjectResult(new Result(false, 404, $"Could not find any games for schedule with Id {scheduleId}.")) { StatusCode = 404 };
            }

            var gameDTOs = new List<GameDTO>();
            foreach (var game in games) 
            {
                var newGameDTO = new GameDTO {
                    GameId = game.Id,
                    ScheduleId = game.ScheduleId,
                    GameDate = game.GameDate,
                    Venue = game.Venue,
                    Opponent = game.Opponent
                };
                gameDTOs.Add(newGameDTO);
            }

            return Ok(new Result(true, 200, "Found Games", gameDTOs));
            
        }

        [HttpPut("gameSchedule/game/{gameId}")]
        public async Task<IActionResult> UpdateGameById(int gameId, [FromBody] GameDTO request)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game is null)
                return new ObjectResult(new Result(false, 404, $"Could not find game with id {gameId}")) { StatusCode = 400 };
        
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(kvp => kvp.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);
        
                return new ObjectResult(errorResponse) { StatusCode = 400 };
            }
            
            var x = _dbHelper.GetGameIdByScheduleIdAndDate(request.ScheduleId, request.GameDate);
            if (x > 0)
                return new ObjectResult(new Result(false, 409, "Game already exists")) { StatusCode = 409 };
            
            game.GameDate = request.GameDate;
            game.Venue = request.Venue;
            game.Opponent = request.Opponent;
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return Ok(new Result(true, 200, "Update Success", _converters.GameToGameDto(game)));
        }
    }
}
