using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using PermissionService.Core.Interfaces;
using UserService.Grpc;
using static PermissionService.API.Permission;

namespace PermissionService.API.Services
{
    public class PermissionGrpcService(IPermissionService permissionService) : PermissionBase
    {
        public override async Task<PermissionResponse> CheckPermission(PermissionRequest request, ServerCallContext context)
        {
            var permission = await permissionService.GetUserPermission(request.UserEmail, request.Room);

            if (permission == null)
            {
                return new PermissionResponse { Role = "None", UserEmail = permission.UserEmail, Room = permission.Room };
            }

            return new PermissionResponse { Role = permission.Role, UserEmail = permission.UserEmail, Room = permission.Room };
        }
    }
}
