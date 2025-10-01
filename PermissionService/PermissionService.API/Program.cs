
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PermissionService.API.Middleware;
using PermissionService.API.Services;
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
            builder.Services.AddGrpc();

            var certPath = Environment.GetEnvironmentVariable("CERT_PATH") ?? "../../Shared/Certs/server.pfx";

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(7122, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                    listenOptions.UseHttps(certPath);
                });
            });

            // URL to your JWKS endpoint
            //var jwksUrl = config["AuthServiceURL"] + "/.well-known/jwks.json";

            //Console.WriteLine($"AuthServiceURL: {jwksUrl}");
            //Console.WriteLine(certPath);

            // Fetch JWKS from AuthService
            //var httpClient = new HttpClient();
            //var jwksJson = await httpClient.GetStringAsync(jwksUrl);
            var authServiceUrl = builder.Configuration["AuthServiceURL"];
            var jwtValidator = new JwtValidator(authServiceUrl!);
            var jwks = await jwtValidator.GetJwksAsync();

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

            app.MapGrpcService<PermissionGrpcService>();

            app.MapHub<PermissionHub>("api/permissionshub");

            app.UseHttpsRedirection();

            app.UseCors("AllowOrigin");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
