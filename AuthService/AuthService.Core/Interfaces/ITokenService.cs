namespace AuthService.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(int userId, string email);
    string GenerateRefreshToken(int userId, string email);
    bool IsTokenValid(string token);
}