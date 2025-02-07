namespace backend.DTO;

public class FoundUserDTO
{
    public int UserId { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    public required string? Email { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string? Role { get; set; }
    public required List<String>? Positions { get; set; }
}