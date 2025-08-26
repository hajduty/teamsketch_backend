using AuthService.Core.Interfaces;

namespace AuthService.Infrastructure.Services;

public class AuthService : IAuthService
{
    public Task<string> LoginAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<string> RefreshTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<string> RegisterAsync(string username, string password)
    {
        throw new NotImplementedException();
    }
}
