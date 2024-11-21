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
		public async Task<IActionResult> SubmitAvailability([FromBody] List<AvailabilityDTO> request)
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

			List<Availability> availabilityList = request.Select(s => new Availability {
				UserId = s.UserId,
				GameId = s.GameId,
				Available = s.Available,
				Comments = s.Comments
			}).ToList();

			await _context.AddRangeAsync(availabilityList);
			await _context.SaveChangesAsync();

			List<AvailabilityDTO> availabilityDTO = [];
			foreach (Availability availability in availabilityList)
			{
				availabilityDTO.Add(new AvailabilityDTO {
					UserId = availability.UserId,
					GameId = availability.GameId,
					Available = availability.Available,
					Comments = availability.Comments
				});
			}

			var response = new Result(true, 200, "Add Success", availabilityDTO);
			return Ok(response);
		}
	}
}
