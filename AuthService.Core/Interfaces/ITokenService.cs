namespace AuthService.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email);
    string GenerateRefreshToken(Guid userId, string email);
    bool IsTokenValid(string token);
}