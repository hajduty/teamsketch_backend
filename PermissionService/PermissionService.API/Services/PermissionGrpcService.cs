using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using PermissionService.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using UserService.Grpc;
using static PermissionService.API.Permission;

namespace PermissionService.API.Services
{
    public class PermissionGrpcService(IPermissionService permissionService) : PermissionBase
    {
        public override async Task<PermissionResponse> CheckPermission(PermissionRequest request, ServerCallContext context)
        {
            var userId = "";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(request.Token);
                var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");
                userId = emailClaim?.Value ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "Id claim not found in token."));
            }
            catch { throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token.")); }

            var permission = await permissionService.GetUserPermission(userId, request.Room);

            if (permission == null)
            {
                return new PermissionResponse { Role = "None", UserId = userId, Room = request.Room };
            }

            return new PermissionResponse { Role = permission.Role, UserId = permission.UserId, Room = permission.Room };
        }
    }
}
