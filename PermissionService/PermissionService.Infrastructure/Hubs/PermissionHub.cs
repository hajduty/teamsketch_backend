using Microsoft.AspNetCore.SignalR;
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

        public async Task<int> GetRoomCount()
        {
            var userId = Context.UserIdentifier;
            if (userId == null)
                throw new HubException("User not authenticated");

            return await _permissionService.GetRoomCountAsync(int.Parse(userId));
        }

        public async Task<IEnumerable<string>> GetRooms()
        {
            var userId = Context.UserIdentifier;
            if (userId == null)
                throw new HubException("User not authenticated");

            return await _permissionService.GetRoomsAsync(int.Parse(userId));
        }

    }
}