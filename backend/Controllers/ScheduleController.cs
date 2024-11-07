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
        public Task<IActionResult> CreateGameSchedule([FromBody] GameScheduleRequest request) {   
            var newSchedule = new Schedule
            {
                Sport = request.Sport,
                Season = request.Season,
                Games = request.Games
            };

            _context.Add(newSchedule);
            _context.SaveChanges();

            var response = new Result(true, 200, "Add Success", newSchedule);
            return Task.FromResult<IActionResult>(Ok(response));
        }

    }
}
