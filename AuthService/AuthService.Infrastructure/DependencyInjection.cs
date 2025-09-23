using AuthService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ITokenService, Services.TokenService>();

        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "https://localhost:7288";

        services.AddGrpcClient<UserService.Grpc.User.UserClient>(o =>
        {
            o.Address = new Uri(userServiceUrl);
        });

        services.AddScoped<IAuthService, Services.AuthService>();

        return services;
    }
}