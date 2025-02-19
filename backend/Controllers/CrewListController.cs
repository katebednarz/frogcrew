using System.Text;
using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class CrewListController : Controller
    {
        
        private readonly FrogcrewContext _context;
        private readonly DtoConverters _converters;

        public CrewListController(FrogcrewContext context)
        {
        _context = context;
        _converters = new DtoConverters(_context);
        }

        /*
            * Finds a crew list by game ID
            * 
            * @param gameId The ID of the game
            * @return The result of the operation
        */
        [HttpGet("crewList/{gameId}")]
        public async Task<IActionResult> FindCrewListById(int gameId) {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }

            return Ok(new Result(true, 200, "Find Success", _converters.GameToCrewListDto(game)));
        }

        [HttpGet("crewList/export/{gameId}")]
        public async Task<IActionResult> ExportCrewList(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }
            
            var crewList = _converters.GameToCrewListDto(game);

            var csv = new StringBuilder();
            csv.AppendLine("Position,Crew Member,ArrivalTime,Location");

            foreach (var crew in crewList.CrewedMembers)
            {
                csv.AppendLine($"{crew.Position},{crew.UserId}");
            }
            
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return Ok(new Result(true, 200, "Export Success", csv.ToString()));
            
        }
    }
}
