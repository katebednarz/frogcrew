using backend.DTO;

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
        [HttpPost("crewSchedule")]
        public async Task<IActionResult> crewSchedule([FromBody] CrewScheduleDTO request)
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
        }
    }
}