using System;

namespace backend.DTO;

public class CrewMemberRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public List<string> Position { get; set; }
}
