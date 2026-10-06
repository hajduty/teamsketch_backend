using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using PermissionService.Core.Interfaces;
using static PermissionService.API.Permission;

namespace PermissionService.API.Services
{
    public class PermissionGrpcService(IPermissionService permissionService, IOptionsMonitor<JwtBearerOptions> jwtOptions) : PermissionBase
    {
        private static readonly JsonWebTokenHandler TokenHandler = new();

        public override async Task<PermissionResponse> CheckPermission(PermissionRequest request, ServerCallContext context)
        {
            // Same signing keys (JWKS), issuer, audience and lifetime rules as the REST endpoints
            var validation = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
            var result = await TokenHandler.ValidateTokenAsync(request.Token, validation);

            if (!result.IsValid)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token."));

            var userId = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "Id claim not found in token."));

            var permission = await permissionService.GetUserPermission(userId, request.Room);

            if (permission == null)
            {
                return new PermissionResponse { Role = "None", UserId = userId, Room = request.Room };
            }

            return new PermissionResponse { Role = permission.Role, UserId = permission.UserId, Room = permission.Room };
        }
    }
}
