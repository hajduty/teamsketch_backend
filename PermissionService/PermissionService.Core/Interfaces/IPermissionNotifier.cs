namespace PermissionService.Core.Interfaces;

public interface IPermissionNotifier
{
    Task NotifyPermissionChanged(int userId, string roomId, string newRole);
    Task NotifyPermissionAdded(int userId, string roomId, string role);
}