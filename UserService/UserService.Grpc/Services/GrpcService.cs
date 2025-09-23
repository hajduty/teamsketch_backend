using Grpc.Core;
using System.Security.Authentication;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;
using static UserService.Grpc.User;

namespace UserService.Grpc.Services
{
    public class GrpcService(IUserService userService) : UserBase
    {
        public override async Task<UserResponse> Login(LoginRequest request, ServerCallContext context)
        {
            try
            {
                var result = await userService.LoginUser(request.Email, request.Password);
                return new UserResponse { Id = result.Id.ToString(), Email = result.Email };
            }
            catch(AuthenticationException ex)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
            }
        }

        public override async Task<UserResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            try
            {
                var result = await userService.RegisterUser(new CreateUserRequest
                {
                    Email = request.Email,
                    Password = request.Password
                });
                return new UserResponse { Id = result.Id.ToString(), Email = result.Email };
            }
            catch(AuthenticationException ex)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
            }
        }
    }
}
