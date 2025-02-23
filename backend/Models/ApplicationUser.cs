using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using backend.DTO;
using backend.Utils;

namespace backend.Models;


public class ApplicationUser : IdentityUser<int>
{
    // IdentityUser<int> already has:
    //   - Id (int)
    //   - Email
    //   - PasswordHash
    //   - PhoneNumber
    
    
    [MaxLength(50)]
    public required string? FirstName { get; set; }
    [MaxLength(50)]
    public required string? LastName { get; set; }
    [MaxLength(25)]
    public string? PayRate { get; set; }

    public bool IsActive { get; set; }
    
    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public virtual ICollection<CrewedUser> CrewedUsers { get; set; } = new List<CrewedUser>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<UserQualifiedPosition> UserQualifiedPositions { get; set; } = new List<UserQualifiedPosition>();
    public virtual ICollection<TradeBoard> TradeBoardDroppers { get; set; } = new List<TradeBoard>();
    public virtual ICollection<TradeBoard> TradeBoardReceivers { get; set; } = new List<TradeBoard>();
    
    // Conversion methods (optional):
    public UserDTO ToUserDTO(FrogcrewContext _context)
    {
        return new UserDTO
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email,
            PhoneNumber = this.PhoneNumber,
            Role = "this.Role",
            Position = PositionToList(_context)
        };
    }
    
    private List<string> PositionToList(FrogcrewContext _context)
	{
        var dbHelper = new DatabaseHelper(_context);
		List<string> list = [];
		foreach (var pos in UserQualifiedPositions)
		{
			list.Add(dbHelper.GetPositionNameById(pos.PositionId));
		}
		return list;
	}

    public UserSimpleDTO ToUserSimpleDTO()
    {
        return new UserSimpleDTO
        {
            UserId = this.Id,
            FullName = $"{this.FirstName} {this.LastName}"
        };
    }

    public List<String> UserPositionToList()
    {
        List<String> positions = new List<String>();
        foreach (var userQualifiedPosition in UserQualifiedPositions)
        {
            positions.Add(userQualifiedPosition.Position.PositionName);
        }
        return positions;
    }
    
}

