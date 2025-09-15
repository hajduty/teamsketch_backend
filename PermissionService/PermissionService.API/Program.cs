
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PermissionService.API.Middleware;
using PermissionService.Infrastructure;
using PermissionService.Infrastructure.Hubs;

namespace PermissionService.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddSignalR();
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

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/api/permissionshub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            var allowedOrigin = builder.Configuration["AllowedOrigin"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", policy =>
                {
                    policy.WithOrigins(allowedOrigin!) // <-- pass origin(s) here
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapHub<PermissionHub>("api/permissionshub");

            app.UseHttpsRedirection();

            app.UseCors("AllowOrigin");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
