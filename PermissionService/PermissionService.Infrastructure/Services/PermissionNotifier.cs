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

        public async Task NotifyPermissionChanged(string user, string room, string role)
        {
            await _hubContext.Clients.User(user)
                .SendAsync("PermissionChanged", new { Room = room, Role = role });
        }

        public async Task NotifyPermissionAdded(string user, string room, string role)
        {
            await _hubContext.Clients.User(user)
                .SendAsync("PermissionAdded", new { Room = room, Role = role, User = user });
        }
    }
}
