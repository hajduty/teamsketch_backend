using AuthService.Core.DTOs;
using E2E.Tests.Fixtures;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using Xunit.Abstractions;

namespace E2E.Tests.Flows
{
    [Collection("Docker collection")]
    public class KickUserFromRoom_Flow
    {
        public DockerServerFixture _fixture;
        private readonly HttpClient _authClient;
        private readonly HttpClient _permClient;
        private readonly HttpClient _roomClient;
        private readonly ITestOutputHelper _output;

        public KickUserFromRoom_Flow(DockerServerFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;

            _authClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_fixture.AuthService.GetMappedPublicPort(8080)}") };
            _permClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_fixture.PermissionService.GetMappedPublicPort(7100)}") };
            _roomClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_fixture.RoomService.GetMappedPublicPort(3002)}") };
        }

        [Fact]
        public async Task KickUserFromRoom_WhenPermissionsRemoved_ShouldCloseWebSocket()
        {
            try
            {
                var user1 = new { Email = "firstuser@example.com", Password = "Password123!" };
                var user2 = new { Email = "seconduser@example.com", Password = "Password123!" };

                var registerUser1 = await _authClient.PostAsJsonAsync("/api/auth/Register", user1);
                var registerUser2 = await _authClient.PostAsJsonAsync("/api/auth/Register", user2);

                var user1Result = await registerUser1.Content.ReadFromJsonAsync<AuthResult>()!;
                var user2Result = await registerUser2.Content.ReadFromJsonAsync<AuthResult>()!;

                Assert.NotNull(user1Result);
                Assert.NotNull(user2Result);

                var user1Element = (JsonElement)user1Result.User;
                var user1Id = user1Element.GetProperty("id").GetGuid();
                var user1Email = user1Element.GetProperty("email").GetString();

                var user2Element = (JsonElement)user2Result.User;
                var user2Id = user2Element.GetProperty("id").GetGuid();
                var user2Email = user2Element.GetProperty("email").GetString();

                _permClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", user1Result.Token);

                var createRoom = await _permClient.PostAsJsonAsync("/api/Permission", new
                {
                    UserId = user1Id,
                    Room = "newRoom",
                    UserEmail = user1Email,
                    Role = "Owner"
                });

                Assert.Equal(HttpStatusCode.OK, createRoom.StatusCode);

                await Task.Delay(400);

                var addUser2 = await _permClient.PostAsJsonAsync("/api/Permission", new
                {
                    Room = "newRoom",
                    UserEmail = user2.Email,
                    Role = "editor"
                });

                Assert.Equal(HttpStatusCode.OK, addUser2.StatusCode);

                await Task.Delay(500);

                var user2WebSocket = new ClientWebSocket();
                var websocketUrl = $"ws://localhost:{_fixture.RoomService.GetMappedPublicPort(3002)}/newRoom/{user2Result.Token}";
                await user2WebSocket.ConnectAsync(new Uri(websocketUrl), CancellationToken.None);
                Assert.Equal(WebSocketState.Open, user2WebSocket.State);

                await Task.Delay(2000);
                var removeUser2 = await _permClient.DeleteAsync($"/api/Permission?roomId=newRoom&userId={user2Id}");
                Assert.Equal(HttpStatusCode.OK, removeUser2.StatusCode);

                await Task.Delay(2000);

                // Keep receiving until we get the close frame
                var buffer = new byte[4096];
                var maxAttempts = 10;
                var attempt = 0;
                WebSocketReceiveResult result;

                do
                {
                    result = await user2WebSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None
                    );
                    attempt++;
                } while (result.MessageType != WebSocketMessageType.Close && attempt < maxAttempts);

                Assert.Equal(WebSocketMessageType.Close, result.MessageType);

                // Acknowledge the close
                await user2WebSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing",
                    CancellationToken.None
                );

                Assert.Equal(WebSocketState.Closed, user2WebSocket.State);
            }
            catch (Exception ex)
            {
                var (stdoute, stderre) = await _fixture.PermissionService.GetLogsAsync();
                var (stdout, stderr) = await _fixture.RoomService.GetLogsAsync();
                Debug.WriteLine("=== RoomService STDOUT ===");
                Debug.WriteLine(stdout);
                Debug.WriteLine("=== RoomService STDERR ===");
                Debug.WriteLine(stderr);
                Debug.WriteLine("=== PermissionService STDOUT ===");
                Debug.WriteLine(stdoute);
                Debug.WriteLine("=== PermissionService STDERR ===");
                Debug.WriteLine(stderre);
                throw;
            }
        }
    }
}
