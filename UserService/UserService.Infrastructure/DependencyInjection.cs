using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        if (!services.Any(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(config.GetConnectionString("DefaultConnection"), ServerVersion.Create(new Version(8, 0, 27), ServerType.MySql)));
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, Services.UserService>();

        return services;
    }
}