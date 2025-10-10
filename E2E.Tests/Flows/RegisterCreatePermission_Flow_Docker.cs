using AuthService.Core.DTOs;
using E2E.Tests.Fixtures;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UserService.Core.DTOs;
using Xunit.Abstractions;

namespace E2E.Tests.Flows
{
    [Collection("Docker collection")]
    public class RegisterFlow
    {
        public DockerServerFixture _fixture;
        private readonly HttpClient _authClient;
        private readonly HttpClient _permClient;
        private readonly ITestOutputHelper _output;

        public RegisterFlow(DockerServerFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _authClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_fixture.AuthService.GetMappedPublicPort(8080)}") };
            _permClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_fixture.PermissionService.GetMappedPublicPort(7100)}")};

            _output = output;
        }

        [Fact]
        public async Task RegisterLoginAndCreateRoom_ReturnsSuccess()
        {
            try
            {
                var req = new { Email = "user@example.com", Password = "Password123!" };
                var registerResp = await _authClient.PostAsJsonAsync("/api/auth/Register", req);
                registerResp.EnsureSuccessStatusCode();

                var loginResp = await _authClient.PostAsJsonAsync("/api/auth/Login", req);
                loginResp.EnsureSuccessStatusCode();
                var loginResult = await loginResp.Content.ReadFromJsonAsync<AuthResult>()!;

                var user = ((JsonElement)loginResult.User).Deserialize<UserResponse>();

                _permClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", loginResult.Token);

                var payload = new
                {
                    Room = "newRoom",
                    UserEmail = "user@example.com",
                    Role = "Owner"
                };

                var create = await _permClient.PostAsJsonAsync("/api/Permission", payload);
                create.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.OK, create.StatusCode);
            }
            catch (Exception ex)
            {
                var (stdout, stderr) = await _fixture.PermissionService.GetLogsAsync();
                Debug.WriteLine("=== CONTAINER STDOUT ===");
                Debug.WriteLine(stdout);
                Debug.WriteLine("=== CONTAINER STDERR ===");
                Debug.WriteLine(stderr);
                throw;
            }
        }
    }
}
