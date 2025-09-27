namespace PermissionService.Core.Interfaces;

public interface IPermissionNotifier
{
    Task NotifyPermissionChanged(string userId, string roomId, string newRole);
    Task NotifyPermissionAdded(string userId, string roomId, string role);
}