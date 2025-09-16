using Microsoft.AspNetCore.SignalR;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using System.Security.Claims;

namespace PermissionService.Infrastructure.Hubs
{
    public class PermissionHub : Hub
    {
        private readonly IPermissionService _permissionService;
        public PermissionHub(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override Task OnConnectedAsync()
        {
            var email = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            Console.WriteLine($"User connected: {email}, Connection ID: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        } 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var email = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            Console.WriteLine($"User  {email}, Connection ID: {Context.ConnectionId} disconnected");

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<List<Permission>> GetRooms()
        {
            var email = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
                throw new HubException("User not authenticated");

            return await _permissionService.GetAllPermissions(email);
        }

        public async Task<Permission> GetPermission(string roomId)
        {
            var email = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == null)
                throw new HubException("User not authenticated");

            var permission = await _permissionService.GetUserPermission(email, roomId);

            if (permission == null)
                throw new HubException("No permission found for this room");
            return permission;
        }

        public async Task<Permission[]> GetPermissionsForRoom(string roomId)
        {
            var email = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == null)
                throw new HubException("User not authenticated");
            var permissions = await _permissionService.GetPermissionsForRoom(roomId, email);
            return permissions.ToArray();
        }
    }
}