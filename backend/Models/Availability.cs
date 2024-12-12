using backend.DTO;

namespace backend.Models;

public partial class Availability
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public int Available { get; set; }

    public string? Comments { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;

	public AvailabilityDTO ConvertToAvailabilityDTO()
	{
		return new AvailabilityDTO
		{
			UserId = UserId,
			GameId = GameId,
			Available = Available == 1,
			Comments = Comments
		};
	}
}
