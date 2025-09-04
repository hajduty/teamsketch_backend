using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Data;
using PermissionService.Infrastructure.Repositories;

namespace PermissionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionService, Services.PermissionService>();

        return services;
    }
}