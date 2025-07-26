namespace UserService.Core.DTOs;

public class CreateUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; } = string.Empty;
}