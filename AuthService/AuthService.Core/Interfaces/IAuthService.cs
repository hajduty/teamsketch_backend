using AuthService.Core.DTOs;

namespace AuthService.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(AuthRequest request);
    Task<AuthResult> RegisterAsync(AuthRequest request);
    Task<AuthResult> RefreshTokenAsync(string token);
}