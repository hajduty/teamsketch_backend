using AuthService.Core.DTOs;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionService.API;
using PermissionService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UserService.Grpc;
using UserService.Infrastructure.Data;

namespace E2E.Tests.Fixtures
{
    public class TestServerFixture : IAsyncLifetime
    {
        // Factories
        public WebApplicationFactory<AuthService.API.Program> AuthFactory { get; private set; }
        public WebApplicationFactory<UserService.Grpc.Program> UserFactory { get; private set; }
        public WebApplicationFactory<PermissionService.API.Program> PermissionFactory { get; private set; }

        // REST clients
        public HttpClient AuthClient { get; private set; }
        public string AuthServiceUrl { get; private set; } = null!;
        public HttpClient PermissionClient { get; private set; }

        // gRPC clients
        public GrpcChannel UserGrpcChannel { get; private set; }
        public User.UserClient UserClient { get; private set; }

        public async Task InitializeAsync()
        {
            UserFactory = new WebApplicationFactory<UserService.Grpc.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var descriptorsToRemove = services
                            .Where(d => d.ServiceType.Namespace?.Contains("Microsoft.EntityFrameworkCore") == true)
                            .ToList();

                        foreach (var descriptor in descriptorsToRemove)
                        {
                            services.Remove(descriptor);
                        }

                        var contextDescriptor = services.SingleOrDefault(d =>
                            d.ServiceType == typeof(UserService.Infrastructure.Data.AppDbContext));
                        if (contextDescriptor != null)
                            services.Remove(contextDescriptor);

                        services.AddDbContext<UserService.Infrastructure.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase("UserTestDb"));
                    });
                });


            var userHttpClient = UserFactory.CreateClient();
            UserGrpcChannel = GrpcChannel.ForAddress(userHttpClient.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = userHttpClient
            });
            UserClient = new User.UserClient(UserGrpcChannel);
            Console.WriteLine("UserService started (in-memory)");

            AuthFactory = new WebApplicationFactory<AuthService.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton(sp =>
                        {
                            return new User.UserClient(UserGrpcChannel);
                        });
                    });
                });

            AuthClient = AuthFactory.CreateClient();
            Console.WriteLine("AuthService started (in-memory)");

            JwtValidator.TestHttpClient = AuthFactory.CreateClient();
            Console.WriteLine("Test HttpClient configured for JwtValidator");

            PermissionFactory = new WebApplicationFactory<PermissionService.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var descriptorsToRemove = services
                            .Where(d => d.ServiceType.Namespace?.Contains("Microsoft.EntityFrameworkCore") == true)
                            .ToList();

                        foreach (var descriptor in descriptorsToRemove)
                        {
                            services.Remove(descriptor);
                        }

                        var contextDescriptor = services.SingleOrDefault(d =>
                            d.ServiceType == typeof(PermissionService.Infrastructure.Data.AppDbContext));
                        if (contextDescriptor != null)
                            services.Remove(contextDescriptor);

                        services.AddDbContext<PermissionService.Infrastructure.Data.AppDbContext>(options =>
                            options.UseInMemoryDatabase("PermissionTestDb"));
                    });
                });

            PermissionClient = PermissionFactory.CreateClient();
            Console.WriteLine("PermissionService started (in-memory)");
        }

        public async Task<string> RegisterAndLoginAsync(string email, string password)
        {
            await AuthClient.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password });
            var loginResp = await AuthClient.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            var loginResult = await loginResp.Content.ReadFromJsonAsync<AuthResult>();
            var token = loginResult!.Token;
            AuthClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            PermissionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return token;
        }

        public Task DisposeAsync()
        {
            AuthFactory?.Dispose();
            UserFactory?.Dispose();
            PermissionFactory?.Dispose();
            UserGrpcChannel?.Dispose();
            AuthClient?.Dispose();
            PermissionClient?.Dispose();
            return Task.CompletedTask;
        }
    }
}
