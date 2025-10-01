using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using AuthService.Infrastructure.Services;
using Grpc.Core;
using Moq;
using System.Threading.Tasks;
using UserService.Grpc;

namespace AuthService.Tests
{
    public class Tests
    {
        private readonly Mock<ITokenService> _tokenService;
        private readonly Mock<User.UserClient> _userClient;
        private readonly Infrastructure.Services.AuthService _authService;

        public Tests()
        {
            _tokenService = new Mock<ITokenService>();
            _userClient = new Mock<User.UserClient>();
            _authService = new Infrastructure.Services.AuthService(_userClient.Object, _tokenService.Object);
        }

        private static AsyncUnaryCall<T> CreateAsyncUnaryCall<T>(T response)
        {
            return new AsyncUnaryCall<T>(
                Task.FromResult(response),
                Task.FromResult(new Grpc.Core.Metadata()),
                () => Grpc.Core.Status.DefaultSuccess,
                () => new Grpc.Core.Metadata(),
                () => { }
            );
        }

        [Fact]
        public async Task Login_ShouldReturnJwt_WhenUserExists()
        {
            _userClient
                .Setup(c => c.LoginAsync(It.IsAny<LoginRequest>(), null, null, default))
                .Returns(CreateAsyncUnaryCall(new UserResponse { Id = "123", Email = "test@example.com"}));

            _tokenService
                .Setup(t => t.GenerateToken("123", "test@example.com"))
                .Returns("jwt-token");

            var result = await _authService.LoginAsync(new Core.DTOs.AuthRequest { Email = "test@example.com", Password = "password123" });

            Assert.True(result.Success);
            Assert.Equal("jwt-token", result.Token);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenUserDoesntExist()
        {
            _userClient
                .Setup(c => c.LoginAsync(It.IsAny<LoginRequest>(), null, null, default))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "User not found")));

            var result = await _authService.LoginAsync(new Core.DTOs.AuthRequest
            {
                Email = "doesnotexist@example.com",
                Password = "wrongpassword"
            });

            Assert.False(result.Success);
            Assert.Equal("User not found", result.ErrorMessage);
            Assert.Null(result.Token);
            Assert.Null(result.User);
        }
    }
}