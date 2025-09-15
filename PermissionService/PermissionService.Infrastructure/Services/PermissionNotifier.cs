using Microsoft.AspNetCore.SignalR;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionService.Infrastructure.Services
{
    public class PermissionNotifier : IPermissionNotifier
    {
        private readonly IHubContext<PermissionHub> _hubContext;
        public PermissionNotifier(IHubContext<PermissionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyPermissionChanged(int userId, string roomId, string role)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("PermissionChanged", new { RoomId = roomId, Role = role });
        }

        public async Task NotifyPermissionAdded(int userId, string roomId, string role)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("PermissionAdded", new { RoomId = roomId, Role = role });
        }
    }
}
