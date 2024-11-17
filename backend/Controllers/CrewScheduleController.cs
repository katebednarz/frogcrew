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

        // POST /crewSchedule
        [HttpPost("crewSchedule/")]
        public async Task<IActionResult> CrewSchedule([FromBody] CrewScheduleDTO request)
        {
            //testing for model binding
            if (request == null)
            {
                return BadRequest("Request body is null");
            }

            if (request.gameId == 0)
            {
                return BadRequest("gameId is 0");
            }


            if (!ModelState.IsValid)
            {
                var errors = ModelState
                        .SelectMany(kvp => kvp.Value!.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);

                return new ObjectResult(errorResponse) { StatusCode = 400 };
            }

            // first check if Game exists
            Game? Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == request.gameId);
            if (Game == null)
            {
                return new ObjectResult(new Result(false, 404, $"Could not find game with ID {request.gameId}.")) { StatusCode = 404 };
            }

            // setting base datetime to Jan 1, 2024
            DateTime arrivalTime = new(2024, 1, 1, 0, 0, 0);
            if (Game.GameDate.HasValue && Game.GameStart.HasValue)
            {
                arrivalTime = Game.GameDate.Value.ToDateTime(TimeOnly.FromTimeSpan(Game.GameStart.Value));
            }

            // set up DTO for later
            CrewScheduleDTO crewScheduleDTO = new()
            {
                gameId = Game.Id,
                changes = []
            };

            foreach (ChangesDTO changes in request.changes)
            {
                // each position has a different start time, so we will calculate those here.
                switch (changes.Position)
                {
                    case "DIRECTOR":
                        arrivalTime = arrivalTime.AddHours(-4);
                        break;
                    case "PRODUCER":
                        arrivalTime = arrivalTime.AddHours(-2);
                        break;
                    case "SOUND":
                        arrivalTime = arrivalTime.AddHours(-1);
                        break;
                    case "CAMERA":
                        arrivalTime = arrivalTime.AddHours(-1);
                        break;
                    default:
                        break;
                }

                // build new model and add to DbContext
                var newCrewedUser = new CrewedUser
                {
                    UserId = changes.Id,
                    GameId = request.gameId,
                    CrewedPosition = changes.Position,
                    ArrivalTime = arrivalTime
                };
                await _context.AddAsync(newCrewedUser);

                // append to master DTO for object
                var user = _context.Users.FirstOrDefault(u => u.Id == changes.Id);
                ChangesDTO changesDTO = new()
                {
                    Action = changes.Action,
                    Id = changes.Id,
                    Position = changes.Position,
                    FullName = $"{user?.FirstName} {user?.LastName}",
                };
                crewScheduleDTO.changes.Add(changesDTO);
            }
            // save and push changes to DB
            await _context.SaveChangesAsync();

            var response = new Result(true, 200, "Crew schedule added", crewScheduleDTO);
            return Ok(response);
        }


        // PUT /crewSchedule/{gameId}
        [HttpPut("crewSchedule/{gameId}")]
        public async Task<IActionResult> UpdateCrewSchedule([FromBody] CrewScheduleDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                        .SelectMany(kvp => kvp.Value!.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                var errorResponse = new Result(false, 400, "Provided arguments are invalid, see data for details.", errors);

                return new ObjectResult(errorResponse) { StatusCode = 400 };
            }

            // first check if Game exists
            Game? Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == request.gameId);
            if (Game == null)
            {
                return new ObjectResult(new Result(false, 404, $"Could not find game with ID {request.gameId}.")) { StatusCode = 404 };
            }

            // set up DTO for later
            CrewScheduleDTO crewScheduleDTO = new()
            {
                gameId = Game.Id,
                changes = []
            };

            foreach (ChangesDTO change in request.changes)
            {
                CrewedUser? crewedUser = await _context.CrewedUsers.FirstOrDefaultAsync(cu => cu.UserId == change.Id && cu.GameId == request.gameId);
                if (crewedUser == null)
                {
                    return new ObjectResult(new Result(false, 404, $"Crewed user with User ID {change.Id} and Game Id {request.gameId} not found.")) { StatusCode = 404 };
                }

                if (change.Action.ToLower() == "assign")
                {
                    crewedUser.CrewedPosition = change.Position;
                    // NEED TO HANDLE CHANGE IN ARRIVAL TIME!
                }
                else if (change.Action.ToLower() == "unassign")
                {
                    _context.CrewedUsers.Remove(crewedUser);
                }

                ChangesDTO changesDTO = new()
                {
                    Action = change.Action,
                    Id = change.Id,
                    Position = change.Position
                };
                crewScheduleDTO.changes.Add(changesDTO);
            }

            // save and push changes to DB
            await _context.SaveChangesAsync();

            var response = new Result(true, 200, "Crew schedule updated", crewScheduleDTO);
            return Ok(response);
        }


        // GET /crewSchedule/{gameId}
        [HttpGet("crewSchedule/{gameId}")]
        public async Task<IActionResult> FindCrewScheduleByGameId(int gameId)
        {
            CrewScheduleDTO crewScheduleDTO = new()
            {
                gameId = gameId,
                changes = []
            };


            var crewedUsers = await _context.CrewedUsers.Where(c => c.GameId == gameId).ToListAsync();
            if (crewedUsers == null)
            {
                return new ObjectResult(new Result(false, 404, $"Crewed users associated with Game ID {gameId} not found.")) { StatusCode = 404 };
            }

            foreach (CrewedUser crewedUser in crewedUsers)
            {
                var user = await _context.Users.FindAsync(crewedUser.UserId);
                if (user == null)
                {
                    return new ObjectResult(new Result(false, 404, $"User with ID {crewedUser.UserId} not found.")) { StatusCode = 404 };
                }

                ChangesDTO changesDTO = new()
                {
                    Id = user.Id,
                    Position = crewedUser.CrewedPosition,
                    FullName = $"{user.FirstName} {user?.LastName}",
                };

                crewScheduleDTO.changes.Add(changesDTO);
            }

            var response = new Result(true, 200, "Crew schedule found", crewScheduleDTO);
            return Ok(response);
        }
    }
}