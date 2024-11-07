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

    }
}
