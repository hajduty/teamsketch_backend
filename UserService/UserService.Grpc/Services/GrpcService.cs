using Grpc.Core;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Services;
using static UserService.Grpc.User;

namespace UserService.Grpc.Services
{
    public class GrpcService : User.UserBase
    {
        private readonly IUserService _userService;
        public GrpcService(IUserService userService)
        {
            this._userService = userService;
        }

        public override async Task<UserResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var result = await _userService.LoginUser(request.Email, request.Password);
            return new UserResponse { Id = result.Id, Email = result.Email };
        }

        public override async Task<UserResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var result = await _userService.RegisterUser(new CreateUserRequest
            {
                Email = request.Email,
                Password = request.Password
            });
            return new UserResponse { Id = result.Id, Email = result.Email };
        }
    }
}
