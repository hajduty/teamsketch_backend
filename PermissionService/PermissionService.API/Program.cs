
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PermissionService.API.Middleware;
using PermissionService.Infrastructure;

namespace PermissionService.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddInfrastructure(config);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // URL to your JWKS endpoint
            var jwksUrl = config["AuthServiceURL"] + "/.well-known/jwks.json";

            // Fetch JWKS from AuthService
            var httpClient = new HttpClient();
            var jwksJson = await httpClient.GetStringAsync(jwksUrl);
            var jwks = new JsonWebKeySet(jwksJson);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "teamsketch", // must match the issuer in your JWT
                        ValidateAudience = true,
                        ValidAudience = "teamsketch_user", // your API audience
                        ValidateLifetime = true,
                        IssuerSigningKeys = jwks.Keys
                    };
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
