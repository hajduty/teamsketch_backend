namespace UserService.Core.DTOs;

public class UserResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
