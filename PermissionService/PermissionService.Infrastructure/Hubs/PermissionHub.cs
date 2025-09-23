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
            var uid = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User connected: {uid}, Connection ID: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        } 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var uid = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User  {uid}, Connection ID: {Context.ConnectionId} disconnected");

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<List<Permission>> GetRooms()
        {
            var uid = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (uid == null)
                throw new HubException("User not authenticated");

            return await _permissionService.GetAllPermissions(uid);
        }

        public async Task<Permission> GetPermission(string roomId)
        {
            var uid = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (uid == null)
                throw new HubException("User not authenticated");

            var permission = await _permissionService.GetUserPermission(uid, roomId);

            if (permission == null)
                throw new HubException("No permission found for this room");
            return permission;
        }

        public async Task<Permission[]> GetPermissionsForRoom(string roomId)
        {
            var uid = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (uid == null)
                throw new HubException("User not authenticated");
            var permissions = await _permissionService.GetPermissionsForRoom(roomId, uid);
            return permissions.ToArray();
        }
    }
}