using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionService.API;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Data;
using PermissionService.Infrastructure.Repositories;
using PermissionService.Infrastructure.Services;

namespace PermissionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        if (!services.Any(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        }

        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "https://localhost:7288";

        services.AddGrpcClient<UserService.Grpc.User.UserClient>(o =>
        {
            o.Address = new Uri(userServiceUrl);
        });

        services.AddSingleton<IPermissionNotifier, PermissionNotifier>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionService, Services.PermissionService>();

        return services;
    }
}