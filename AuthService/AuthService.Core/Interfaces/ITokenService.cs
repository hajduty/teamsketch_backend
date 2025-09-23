namespace AuthService.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email);
    string GenerateRefreshToken(string userId, string email);
    bool IsTokenValid(string token);
}