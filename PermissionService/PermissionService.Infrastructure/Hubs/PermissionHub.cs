using Microsoft.AspNetCore.SignalR;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;

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
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User connected: {userId}, Connection ID: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        } 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User  {userId}, Connection ID: {Context.ConnectionId} disconnected");

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<List<Permission>> GetRooms()
        {
            var userId = Context.UserIdentifier;
            if (userId == null)
                throw new HubException("User not authenticated");

            return await _permissionService.GetAllPermissions(int.Parse(userId));
        }

        public async Task<Permission> GetPermission(string roomId)
        {
            var userId = Context.UserIdentifier;

            if (userId == null)
                throw new HubException("User not authenticated");

            var permission = await _permissionService.GetUserPermission(int.Parse(userId), roomId);

            if (permission == null)
                throw new HubException("No permission found for this room");
            return permission;
        }

        public async Task<Permission[]> GetPermissionsForRoom(string roomId)
        {
            var userId = Context.UserIdentifier;
            if (userId == null)
                throw new HubException("User not authenticated");
            var permissions = await _permissionService.GetPermissionsForRoom(roomId, int.Parse(userId));
            return permissions.ToArray();
        }
    }
}