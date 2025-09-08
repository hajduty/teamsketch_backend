using AuthService.Core.Entities;

namespace AuthService.Core.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public object? User { get; set; }
    public string? ErrorMessage { get; set; }
}
