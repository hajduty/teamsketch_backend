using E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using PermissionService.Infrastructure.Migrations;
using System.Net.Http.Json;
using UserService.Grpc;

namespace E2E.Tests.Flows
{
    public class RegisterCreateRoom_Flow : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        public RegisterCreateRoom_Flow(TestServerFixture fixture)
        {
            _fixture = fixture;
        }

        /*
        [Fact]
        public async Task PermissionService_IsRunning()
        {
            var response = await _fixture.PermissionClient.GetAsync("/");
            Console.WriteLine($"Base endpoint status: {response.StatusCode}");

            var healthResponse = await _fixture.PermissionClient.GetAsync("/health");
            Console.WriteLine($"Health endpoint status: {healthResponse.StatusCode}");

            Console.WriteLine($"Base Address: {_fixture.PermissionClient.BaseAddress}");
        }

        [Fact]
        public async Task ListAllRoutes()
        {
            var endpoints = _fixture.PermissionFactory.Services
                .GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>()
                .Endpoints;

            foreach (var endpoint in endpoints)
            {
                if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
                {
                    Console.WriteLine($"Route: {routeEndpoint.RoutePattern.RawText}");
                }
            }
        }*/

        [Fact]
        public async Task Register_Login_CreateRoom_Works()
        {
            var token = await _fixture.RegisterAndLoginAsync("user@test.com", "Password123!");
            Console.WriteLine($"Token: {token}");

            var userResponse = await _fixture.UserClient.EmailToUidAsync(new EmailToUidRequest
            {
                Email = "user@test.com"
            });
            Console.WriteLine($"User UID: {userResponse.Id}");

            var payload = new
            {
                Room = "room1",
                UserEmail = "user@test.com",
                Role = "Owner"
            };
            Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            Console.WriteLine($"URL: {_fixture.PermissionClient.BaseAddress}/api/Permission");

            var createRoomResp = await _fixture.PermissionClient.PostAsJsonAsync("/api/Permission", payload);

            Console.WriteLine($"Status: {createRoomResp.StatusCode}");
            var responseBody = await createRoomResp.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {responseBody}");

            //Test
            createRoomResp.StatusCode = System.Net.HttpStatusCode.Forbidden;

            createRoomResp.EnsureSuccessStatusCode();
        }
    }
}