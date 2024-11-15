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
        public AvailabilityController(FrogcrewContext context)
        {
            _context = context;
        }

        // POST /crewSchedule
        [HttpPost("crewschedule/")]
        public async Task<IActionResult> CrewSchedule([FromBody] CrewScheduleDTO request)
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
            Game? Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == request.Id);
            if (Game == null)
            {
                return NotFound($"Game with ID {request.Id} not found.");
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
                Id = Game.Id
            };

            foreach (ChangesDTO changes in request.Changes)
            {
                // each position has a different start time, so we will calculate those here.
                switch (changes.Position)
                {
                    case "DIRECTOR":
                        arrivalTime.AddHours(-4);
                        break;
                    case "PRODUCER":
                        arrivalTime.AddHours(-2);
                        break;
                    case "SOUND":
                        arrivalTime.AddHours(-1);
                        break;
                    case "CAMERA":
                        arrivalTime.AddHours(-1);
                        break;
                    default:
                        break;
                }

                // build new model and add to DbContext
                var newCrewedUser = new CrewedUser
                {
                    UserId = changes.Id,
                    GameId = request.Id,
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
                    FullName = $"{user.FirstName} {user.LastName}",
                };
                crewScheduleDTO.Changes.Add(changesDTO);
            }
            // save and push changes to DB
            await _context.SaveChangesAsync();

            var response = new Result(true, 200, "Crew schedule added", crewScheduleDTO);
            return Ok(response);
        }
    }
}