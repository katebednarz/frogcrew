using System;
using System.Collections.Generic;
using backend.DTO;

namespace backend.Models;

public partial class User
{
	public int Id { get; set; }

	public string Email { get; set; } = null!;

	public string Password { get; set; } = null!;

	public string? PhoneNumber { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? Role { get; set; }

	public string? PayRate { get; set; }

	public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

	public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();

	public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

	public virtual ICollection<UserQualifiedPosition> UserQualifiedPositions { get; set; } = new List<UserQualifiedPosition>();

	public UserDTO ConvertToUserDTO()
	{
		return new UserDTO
		{
			Id = Id,
			FirstName = FirstName,
			LastName = LastName,
			Email = Email,
			PhoneNumber = PhoneNumber,
			Role = Role,
			Position = PositionToList()
		};
	}

	private List<string> PositionToList()
	{
		List<string> list = [];
		foreach (var pos in UserQualifiedPositions)
		{
			list.Add(pos.Position);
		}
		return list;
	}

	public UserSimpleDTO ConvertToUserSimpleDTO() {
		return new UserSimpleDTO {
			UserId = Id,
			FullName = $"{FirstName} {LastName}"
		};
	}
}
