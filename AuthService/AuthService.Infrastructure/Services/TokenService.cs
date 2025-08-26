using AuthService.Core.Interfaces;

namespace AuthService.Infrastructure.Services;

public class TokenService : ITokenService
{
    string ITokenService.GenerateRefreshToken(Guid userId, string email)
    {
        throw new NotImplementedException();
    }

    string ITokenService.GenerateToken(Guid userId, string email)
    {
        throw new NotImplementedException();
    }

    bool ITokenService.IsTokenValid(string token)
    {
        throw new NotImplementedException();
    }
}