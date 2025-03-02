using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using backend.Utils;
using Microsoft.EntityFrameworkCore;


namespace backend.Controllers
{
	[Route("/")]
	[ApiController]
	public class AvailabilityController : Controller
	{
		private readonly FrogcrewContext _context;
		private readonly NotificationsHelper _notificationsHelper;
		public AvailabilityController(FrogcrewContext context, NotificationsHelper notificationsHelper)
		{
			_context = context;
			_notificationsHelper = notificationsHelper;
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
			var user = _context.Users.Find(request[0].UserId);
			var game = await _context.Games.Include(g => g.Schedule).FirstOrDefaultAsync(g => g.Id == request[0].GameId);
			
			string notificationMessage = NotificationContent.GetNotificationTemplate("AvailabiltyPostedNotification", [user.FirstName, user.LastName, availabilityList.Count, game.Schedule.Sport]);
			_notificationsHelper.SendNotificationToAdmin(notificationMessage);
			var response = new Result(true, 200, "Add Success", availabilityDTO);
			return Ok(response);
		}
	}
}
