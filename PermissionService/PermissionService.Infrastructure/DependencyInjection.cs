using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionService.API;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Data;
using PermissionService.Infrastructure.Repositories;
using PermissionService.Infrastructure.Services;
using StackExchange.Redis;
using Pomelo.EntityFrameworkCore.MySql;

namespace PermissionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSignalR()
            .AddStackExchangeRedis(config.GetConnectionString("RedisConnectionString"), options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("SignalR");
            });

        if (!services.Any(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)))
        {
            services.AddDbContext<AppDbContext>(options => 
            options.UseMySql(config.GetConnectionString("DefaultConnection"), ServerVersion.Create(new Version(8, 0, 27), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)));

        }

        services.AddGrpcClient<UserService.Grpc.User.UserClient>(o =>
        {
            o.Address = new Uri(config["UserServiceURL"]);
        });

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(config.GetConnectionString("RedisConnectionString"))
        );

        services.AddSingleton<IPermissionPublisher, PermissionPublisher>();

        services.AddSingleton<IPermissionNotifier, PermissionNotifier>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionService, Services.PermissionService>();

        return services;
    }
}