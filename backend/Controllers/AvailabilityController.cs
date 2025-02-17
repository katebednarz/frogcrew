using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using Microsoft.EntityFrameworkCore;


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

		/*
			* Adds availability for a user
			* 
			* @param request The availability to add
			* @return The result of the operation
		*/
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
				Available = s.Available ? 1 : 0,
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
					Available = availability.Available == 1,
					Comments = availability.Comments
				});
			}

			var response = new Result(true, 200, "Add Success", availabilityDTO);
			return Ok(response);
		}
		
		/*
			* Adds availability for a user
			* 
			* @param request The availability to add
			* @return The result of the operation
		*/
		[HttpGet("availability/{userId}")]
		public async Task<IActionResult> GetAvailability(int userId)
		{
			if (userId == 0)
			{
				var errorResponse = new Result(false, 400, "User ID is required.", null);
				return new ObjectResult(errorResponse) { StatusCode = 400 };
			}
			
			var availabilityList = await _context.Availabilities
				.Where(a => a.UserId == userId)
				.Select(a => new AvailabilityDTO
				{
					UserId = a.UserId,
					GameId = a.GameId,
					Available = a.Available == 1,
					Comments = a.Comments
				})
				.ToListAsync();

			if (availabilityList.Count == 0)
			{
				var notFoundResponse = new Result(false, 404, $"Could not find user with id {userId}", null);
				return new ObjectResult(notFoundResponse) { StatusCode = 404 };
			}

			var successResponse = new Result(true, 200, "Find Success", availabilityList);
			return Ok(successResponse);
		}
	}
}
