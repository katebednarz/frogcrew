using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;


namespace backend.Controllers
{
	[Route("/")]
	[ApiController]
	public class AvailabilityController : Controller
	{
		private readonly FrogcrewContext _context;
		public AvailabilityController(FrogcrewContext context)
		{
			_context = context;
		}

		// POST /availability
		[HttpPost("availability")]
		public async Task<IActionResult> SubmitAvailability([FromBody] AvailabilityDTO request)
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

			var newAvailability = new Availability
			{
				UserId = request.UserId,
				GameId = request.GameId,
				Open = request.Open,
				Comment = request.Comment
			};

			_context.Add(newAvailability);
			_context.SaveChanges();

			var response = new Result(true, 200, "Add Success", newAvailability.ConvertToAvailabilityDTO());
			return Ok(response);
		}
	}
}
