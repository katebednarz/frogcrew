using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class CrewedUserController : Controller
    {

        private readonly FrogcrewContext _context;
        private readonly DatabaseHelper _dbHelper;
        
        public CrewedUserController(FrogcrewContext context)
        {
            _context = context;
            _dbHelper = new DatabaseHelper(context);
        }

        /*
         * Return a list of crew member who are qualified for the position and available for the game.
         *
         * @param request ID# of Game and Position to search for
         * @return List of qualified crew members based on criteria
         */
        [HttpGet("crewMember/{gameId:int}/{position}")]
        public async Task<IActionResult> FindCrewMemberByGameAndPosition(int gameId, string position)
        {

            var positionId = _dbHelper.GetPositionIdByName(position);
            
            var availableQualifiedUsers = await _context.Users
                .Where(u => u.Availabilities.Any(a => a.GameId == gameId && a.Available == 1)
                            && u.UserQualifiedPositions.Any(qp => qp.PositionId == positionId))
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

        
        
        
        
        
        
        /*
         * Update crew members attached to game and return action.
         *
         * @param request ID# of Game and list of new crew members to add
         * @return Result of new data generated
         */
        
        /*
         * gameId will be passed as a parameter instead of in the body
         *
         *          "crewedUsers": [
         *              {
         *                  "UserId": 1,
         *                  "Position": "DIRECTOR",
         *              },
         *              {
         *                  "UserId": 2,
         *                  "Position": "PRODUCER",
         *              }
         *          ]
         */
        
        [HttpPost("crewedUser/{gameId:int}")]
        public async Task<IActionResult> UpdateCrewedUsers(int gameId, [FromBody] List<CrewedUserDTO> crewedUsers)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return NotFound(new Result(false, 404, $"Game with ID {gameId} not found.", null!));
            
            // Pull game start time from Game; if not, provide default.
            TimeOnly? startTime = new TimeOnly(12, 0, 0, 0);
            if (game.GameStart != null)
            {
                startTime = game.GameStart;
            }
            // Set up for out return object on success.
            var returnDto = new CrewedUserDTO
            {
                GameId = gameId,
                Position = "",
            };

            // Update arrival times of each CrewedUser based on position.
            foreach (var crewedUser in crewedUsers)
            {
                TimeOnly? arrivalTime = crewedUser.Position switch
                {
                    "DIRECTOR" => startTime.Value.AddHours(-4),
                    "PRODUCER" => startTime.Value.AddHours(-2),
                    "SOUND" or "CAMERA" => startTime.Value.AddHours(-1),
                    _ => startTime.Value
                };
                
                var position = _context.Positions.FirstOrDefault(p => p.PositionName == crewedUser.Position)?.PositionId;
                if (position == null)
                    return NotFound(new Result(false, 404, $"Position {crewedUser.Position} not found.", null!));

                var newCrewedUser = new CrewedUser
                {
                    UserId = crewedUser.UserId,
                    GameId = gameId,
                    PositionId = (int)position,
                    ArrivalTime = new TimeOnly(arrivalTime.Value.Hour,
                        arrivalTime.Value.Minute,
                        arrivalTime.Value.Second,
                        arrivalTime.Value.Millisecond)
                };

                returnDto = new CrewedUserDTO
                {
                    UserId = crewedUser.UserId,
                    GameId = gameId,
                    Position = crewedUser.Position,
                };
                
                // Append updates to CrewedUser table.
                await _context.AddAsync(newCrewedUser);
            }
            
            var response = new Result(true, 200, "Crewed users created.", returnDto);
            return Ok(response);
        }
    }
}
