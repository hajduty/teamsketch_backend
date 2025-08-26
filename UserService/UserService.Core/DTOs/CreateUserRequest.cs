namespace UserService.Core.DTOs;

public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; } = string.Empty;
}