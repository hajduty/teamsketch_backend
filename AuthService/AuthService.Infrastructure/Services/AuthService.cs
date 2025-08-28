using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using Grpc.Core;
using UserService.Grpc;

namespace AuthService.Infrastructure.Services;

public class AuthService(User.UserClient userClient, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResult> LoginAsync(AuthRequest user)
    {
        try
        {
            var request = new LoginRequest { Email = user.Email, Password = user.Password };
            var response = await userClient.LoginAsync(request);

            if (response == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid credentials." };
            }

            var token = tokenService.GenerateToken(response.Id, response.Email);
            return new AuthResult { Success = true, Token = token };
        }
        catch (RpcException ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<AuthResult> RefreshTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResult> RegisterAsync(AuthRequest user)
    {
        try
        {
            var request = new RegisterRequest { Email = user.Email, Password = user.Password };
            var response = await userClient.RegisterAsync(request);

            if (response == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid credentials." };
            }

            var token = tokenService.GenerateToken(response.Id, response.Email);
            return new AuthResult { Success = true, Token = token };
        }
        catch (RpcException ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
