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
            
            // If no arrival time has been specified, we will set a default date here.
            DateTime arrivalTime = new(2024, 1, 1, 0, 0, 0);
            if (game is { GameDate: not null, GameStart: not null })
            {
                var gameStart = game.GameStart.Value;
                var gameDate = game.GameDate.Value.ToDateTime(TimeOnly.MinValue);
                arrivalTime = gameDate.AddHours(gameStart.Hour)
                    .AddMinutes(gameStart.Minute)
                    .AddSeconds(gameStart.Second)
                    .AddMilliseconds(gameStart.Millisecond);
            }

            // Set up for out return object on success.
            var returnDto = new CrewedUserDTO
            {
                Position = null!,
                ArrivalTime = null
            };

            // Update arrival times of each CrewedUser based on position.
            foreach (var crewedUser in crewedUsers)
            {
                arrivalTime = crewedUser.Position switch
                {
                    "DIRECTOR" => arrivalTime.AddHours(-4),
                    "PRODUCER" => arrivalTime.AddHours(-2),
                    "SOUND" or "CAMERA" => arrivalTime.AddHours(-1),
                    _ => arrivalTime
                };
                
                var position = _context.Positions.FirstOrDefault(p => p.PositionName == crewedUser.Position)?.PositionId;
                if (position == null)
                    return NotFound(new Result(false, 404, $"Position {crewedUser.Position} not found.", null!));

                var newCrewedUser = new CrewedUser
                {
                    UserId = crewedUser.UserId,
                    GameId = gameId,
                    PositionId = (int)position,
                    ArrivalTime = new TimeOnly(arrivalTime.Hour, arrivalTime.Minute, arrivalTime.Second,
                        arrivalTime.Millisecond)
                };

                returnDto = new CrewedUserDTO
                {
                    UserId = crewedUser.UserId,
                    GameId = gameId,
                    Position = crewedUser.Position,
                    ArrivalTime = newCrewedUser.ArrivalTime.ToString()
                };
                
                // Append updates to CrewedUser table.
                await _context.AddAsync(newCrewedUser);
            }
            
            var response = new Result(true, 200, "Crewed users created.", returnDto);
            return Ok(response);
        }
    }
}
