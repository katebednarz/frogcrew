namespace backend.DTO;

public class AuthDTO
{
    public int UserId { get; set; }
    public required string Role { get; set; }
    public required string Token { get; set; }
    
}