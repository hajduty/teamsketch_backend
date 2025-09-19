using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using PermissionService.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using UserService.Grpc;
using static PermissionService.API.Permission;

namespace PermissionService.API.Services
{
    public class PermissionGrpcService(IPermissionService permissionService) : PermissionBase
    {
        public override async Task<PermissionResponse> CheckPermission(PermissionRequest request, ServerCallContext context)
        {
            var userEmail = "";
            Console.WriteLine("HELLOOOOOOO FROM PERMISSIONSERVICE");
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(request.Token);
                var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "email");
                userEmail = emailClaim?.Value ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "Email claim not found in token."));
            }
            catch { throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token.")); }

            var permission = await permissionService.GetUserPermission(userEmail, request.Room);

            if (permission == null)
            {
                return new PermissionResponse { Role = "None", UserEmail = userEmail, Room = request.Room };
            }

            return new PermissionResponse { Role = permission.Role, UserEmail = permission.UserEmail, Room = permission.Room };
        }
    }
}
