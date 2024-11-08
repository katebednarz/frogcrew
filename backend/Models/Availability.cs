using backend.DTO;

namespace backend.Models;

public partial class Availability
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public bool Open { get; set; }

    public string? Comment { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;

	public AvailabilityDTO ConvertToAvailabilityDTO()
	{
		return new AvailabilityDTO
		{
			UserId = UserId,
			GameId = GameId,
			Open = Open,
			Comment = Comment
		};
	}
}
